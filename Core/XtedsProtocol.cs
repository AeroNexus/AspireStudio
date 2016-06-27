using System;
using System.Collections.Generic;
using System.Threading;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	public class XtedsProtocol : Protocol
	{
		public delegate void UnknownComponentHandler(Message message, MarshaledBuffer sbuf);
		protected UnknownComponentHandler mHandleUnknownComponent;
		List<XComponent> mComponents = new List<XComponent>();
		List<ThreadHeader> headerByThread = new List<ThreadHeader>();

		List<DeferredMsg> activeDeferredMsgs;
		List<DeferredMsg> deferredMessages = new List<DeferredMsg>();
		int activeDeferredIndex = -1, pendingDeliveries;
		Address mOwnAddress;

		Continuation ackCont = new Continuation();
		Message mParseMessage, mSendMessage;

		SecondaryHeaderIterator mSecHdrIter = new SecondaryHeaderIterator();
		Message mContHdr;

		public XtedsProtocol(IXteds xteds, Transport transport) :
			base(ProtocolId.Xteds, transport)
		{
			mXteds = xteds;

			mParseMessage = ProtocolFactory.CreateMessage(Id);
			mParseMessage.Version = (byte)Id;

			mSendMessage = ProtocolFactory.CreateMessage(Id);
			mSendMessage.Version = (byte)Id;
			mSendMessage.Source = transport.ListenAddress;
			mOwnAddress = mSendMessage.Source;

			mContHdr = mSendMessage.Clone();
			mContHdr.Version = (byte)ProtocolId.XtedsData;
			mContHdr.Source = mSendMessage.Source; // So id gets updated when registered.
			mContHdr.HeaderLength = ackCont.MarshalSize();
		}

		// Needed by SDT
		//public void AddComponent(Address provider, IXteds xteds)
		//{
		//	var comp = FindComponent(provider);
		//	if ( comp == null )
		//	{
		//		comp = new XComponent(provider);
		//		mComponents.Add(comp);
		//	}
		//	comp.mXteds = xteds;
		//}

		// Needed by SDT
		//public void AddMessage(Address provider, XtedsMessage msg)
		//{
		//	XComponent comp = FindComponent(provider);
		//	if (comp == null)
		//	{
		//		comp = new XComponent(provider);
		//		//Log.WriteLine("Adding XComp {0} to comp {1}", provider.ToString(), mOwnAddress.ToString());
		//		mComponents.Add(comp);
		//	}
		//	//Log.WriteLine("XtedsMsg from {0} to comp {1}", provider.ToString(), mOwnAddress.ToString());
		//	//Log.WriteLine("[{0}] {1}", messageSpec.Length, messageSpec.ToString());

		//	comp.Xteds.AddMessage(msg);
		//	msg.Provider = provider;
		//}

		public string AppName { get; set; }

		public void CancelAllSubscriptions(ControlProtocol controlProtocol)
		{
			var appProto = controlProtocol as ApplicationProtocol;

			int dialog = 1;
			foreach (var comp in mComponents)
				foreach (var dmsg in comp.Xteds.NotificationMessages())
					if (dmsg.Publication != null)
						appProto.SendSubscriptionCancel(dmsg.Provider, dmsg.MessageId, null, dialog++);
		}

		public void ChangeMessageProviders(bool active, Address address, Address prevAddress,
			ChangePublisherState OnChangePublisherState)
		{
			if ( active )
			{
				XComponent comp = FindComponent(prevAddress);
				if ( comp != null )
				{
					comp.Address = address.Clone();
					comp.Xteds.ChangeAllCommandProviders(comp.Address);
					comp.Xteds.VisitAllPublishers(OnChangePublisherState,active,comp.Address);
				}
			}
			else
			{
				XComponent comp = FindComponent(address);
				if ( comp != null )
					comp.Xteds.VisitAllPublishers(OnChangePublisherState,active,comp.Address);
			}
		}

		public void Clear()
		{
			mComponents.Clear();
		}

		public Message CloneHeader()
		{
			return mSendMessage.Clone();
		}

		private void Deliver(int limit)
		{
			// Just to be safe
			if ( activeDeferredIndex == -1 )
				return;

			// process the lot
			while (activeDeferredIndex < activeDeferredMsgs.Count && limit-- > 0)
			{
				var deferred = activeDeferredMsgs[activeDeferredIndex];
				if ( deferred.xtedsMessage.Disposition != XtedsMessage.DispositionType.Defer )
				{
					Parse(deferred.buffer, (int)deferred.length,null);
					deferred.Dispose();
					deferredMessages.RemoveAt(activeDeferredIndex);
				}
				else
					activeDeferredIndex++;
			}

			// if at end, re-initialize if requests came in while delivering the previous lot
			if ( activeDeferredIndex == deferredMessages.Count )
			{
				activeDeferredMsgs.Clear();
				activeDeferredMsgs = null;
				if ( pendingDeliveries > 0 )
					InitiateDelivery(limit);
			}
		}

		public XComponent FindComponent(Address address)
		{
			for( int i=0; i<mComponents.Count; i++)
			{
				var xComp = mComponents[i];
				// domain id is now part of the hash
				if (xComp != null && xComp.Address.Hash == address.Hash)
				{
					if (FindComponentWithWholeAddress)
					{
						// if multiple domains, disambiguate hash
						if (xComp.Address.EqualsNetwork(address))
							return xComp;
					}
					else // Hash is sufficient; using domainId
						return xComp;
				}
			}
			return null;
		}

		public bool FindComponentWithWholeAddress { get; set; }

		public XtedsMessage FindMessage(Address providerAddress, MarshaledBuffer messageSpec, Address domainAddress)
		{
			XComponent comp = FindComponent(providerAddress);
			if (comp == null)
			{
				comp = new XComponent(providerAddress.Clone(), domainAddress);
				//Log.WriteLine("Adding XComp {0} to comp {1}", provider.ToString(), mOwnAddress.ToString());
				mComponents.Add(comp);
			}
			//Log.WriteLine("XtedsMsg from {0} to comp {1}", provider.ToString(), mOwnAddress.ToString());
			//Log.WriteLine("[{0}] {1}", messageSpec.Length, messageSpec.ToString());

			XtedsMessage xmsg = comp.Xteds.FindMessage(messageSpec);
			return xmsg;
		}

		public XtedsMessage FindMessage(Address provider, int messageSelector)
		{
			XComponent comp = FindComponent(provider);
			if (comp == null) return null;
			XtedsMessage xmsg = comp.Xteds.FindMessage(messageSelector);
			return xmsg;
		}

		public UInt32 Hash
		{
			set
			{
				mSendMessage.Source.Hash = value;
				mContHdr.Source.Hash = value;
			}
		}

		byte[] nullMsg = new byte[24];

		public void InitiateDelivery(int count)
		{
			if ( activeDeferredMsgs == null )
			{
				activeDeferredMsgs = deferredMessages;
				deferredMessages = new List<DeferredMsg>();
				pendingDeliveries = 0;
				if ( count > 255 ) count = 255;
				// We send ourselves a message so MessageHandler doesn't need to understand this 
				// special need. msgSelector=0 => Deferred msgId = <0.0>
				base.Send(mSendMessage, 0, nullMsg, 0, mSendMessage.Size,mOwnAddress,count);
			}
			else
				pendingDeliveries++;
		}

		DeferredMsg marshalDm = new DeferredMsg();

		public DeferredMsg Marshal(XtedsMessage xMessage, Address destination, int sequence)
		{
			byte[] bytes = sendBuffer;
			marshalDm.Set(xMessage, bytes, bytes.Length, destination, sequence);
			if (Marshal(marshalDm))
				return new DeferredMsg(marshalDm);
			else
				return null;
		}

		bool Marshal(DeferredMsg deferred)
		{
			var xMessage = deferred.xtedsMessage;
			if (!xMessage.HeaderIsSet)
			{
				Message hdr = ThisThreadsHeader(this);
				//mSendMessage;
				xMessage.Header = hdr;
			}
			var header = xMessage.Header;
			int fixedHdrSize = header.Size, payloadOffset = fixedHdrSize;
			uint bufferSize;

			var secHdr = xMessage.SecondaryHeaders;
			if ( secHdr != null )
			{
				byte[] buf = deferred.buffer;
				int offset = fixedHdrSize;
				bufferSize = (uint)(deferred.length-fixedHdrSize);

				// Marshal all the secondary headers
				deferred.payloadOffset = offset;
				offset = xMessage.MarshalSecondaryHeaders(buf, offset);
				int secHdrLength = offset - deferred.payloadOffset, m = secHdrLength % 4;
				if ( m != 0 ) // force secondary headers to 4 byte quanta
				{
					buf[offset] = 0;
					offset += 4-m;
					secHdrLength = offset-deferred.payloadOffset;
				}
				header.PushHeaderSize((byte)((fixedHdrSize+secHdrLength)>>2));
				deferred.payloadLength = (uint)(xMessage.Marshal(buf,offset,(uint)(bufferSize-secHdrLength)) + secHdrLength);
				// For now, Marshal differs from Send in that it does NOT automatically handle
				// large payloads. The issue is when a Continuation SH is added to the XtedsMessage,
				// that gets shared and overwritten for each DeferredMsg instance associated with the XtedsMessage.
				if ( deferred.payloadLength > bufferSize )
					return false;
			}
			else
			{
				deferred.payloadOffset = fixedHdrSize;
				deferred.payloadLength = (uint)xMessage.Marshal(deferred.buffer, deferred.payloadOffset, (uint)(deferred.length - fixedHdrSize));
				// For now, Marshal differs from Send in that it does NOT automatically handle
				// large payloads. The issue is when a Continuation SH is added to the XtedsMessage,
				// that gets shared and overwritten for each DeferredMsg instance associated with the XtedsMessage.
				if ( deferred.payloadLength >= deferred.length - header.HeaderLength )
					return false;
			}

			if ( xMessage is IDataMessage || xMessage.ReplyMessage == xMessage)
				header.Version = (byte)ProtocolId.XtedsData;
			else
				header.Version = (byte)ProtocolId.XtedsCommand;
			if ( deferred.destination == null )
				deferred.destination = xMessage.Provider;
			if ( deferred.destination == null )
			{
				if ( xMessage is IDataMessage )
					Logger.Log(1,"{0} is a Notification data message;get it as a DataMessage from OwnDataMessage() and Publish(msg) instead of Send() \n",
						xMessage.Name);
				else
					Logger.Log(1,"Can't send {0} to unknown destination.\n",xMessage.Name);
				return false;
			}

			deferred.length = (uint)(deferred.payloadOffset + deferred.payloadLength);
			if (secHdr != null) header.PopHeaderSize();
			return true;
		}

		//public override int MessageSize { get { return mSendMessage.Size; } }

		MarshaledBuffer sbuf = new MarshaledBuffer();
		byte[] mAckBuf = new byte[64];

		public override bool Parse(byte[] buffer, int length, Message parseMessage)
		{
			if (parseMessage==null) parseMessage = mParseMessage;

			int offset = parseMessage.Unmarshal(buffer,0,length);
			if ( parseMessage.Version == (byte)ProtocolId.XtedsCommand )
			{
				XtedsMessage xMsg = mXteds.FindMessage(parseMessage.Selector,false);
				if (xMsg == null && parseMessage.HaveSecondaryHeaders)
				{
					var comp = FindComponent(parseMessage.Source);
					if (comp != null)
						xMsg = comp.Xteds.FindMessage(parseMessage.Selector);
				}
				if (xMsg != null)
				{
					if ( parseMessage.HaveSecondaryHeaders )
					{
						mSecHdrIter.SetHeader(parseMessage);
						// If there is a Continuation, it is the acknowledge for the previously
						// transmitted packet. Send the next packet if required.
						if (mSecHdrIter.Has(SecondaryHeader.SecondaryHdrType.Continuation))
						{
							var continuation = Continuation.FromMessage(xMsg);
							mContHdr.Version = (byte)ProtocolId.XtedsCommand;
							if (continuation.IsTransmitter)
							{
								int startNextPacket;
								int nextLen = continuation.NeedToTransmit(mSecHdrIter, out startNextPacket);
								if (nextLen > 0)
								{
									mContHdr.Version = (byte)ProtocolId.XtedsCommand;
									mContHdr.Source = parseMessage.Destination;
									Send(mContHdr, parseMessage.Selector, continuation.PayloadBuffer,
										startNextPacket, nextLen, parseMessage.Source, parseMessage.Sequence);
								}
								return false;
							}
							else if (mSecHdrIter.GetSecondaryHeader(continuation) != null)
							{
								int ackLen = continuation.AddPacket(buffer, offset, length - offset, mAckBuf);
								if (ackLen > 0) // all subsequent packets
									mTransport.Write(mAckBuf, 0, ackLen + mContHdr.Size, parseMessage.Source);
								else // first packet
								{
									mContHdr.Source = parseMessage.Destination;
									base.Send(mContHdr, parseMessage.Selector, mAckBuf, mContHdr.Size,
										-ackLen, parseMessage.Source, parseMessage.Sequence);
								}
								if (!continuation.Finished)
									return false;
								// Setup for actual message delivery
								buffer = continuation.PayloadBuffer;
								length = (int)continuation.PayloadLength;
							}
						}
					}

					if ( xMsg.Disposition != XtedsMessage.DispositionType.Deliver )
					{
						if ( xMsg.Disposition == XtedsMessage.DispositionType.Defer )
							deferredMessages.Add(new DeferredMsg(xMsg,buffer,offset,length));
						return false;
					}
					sbuf.Set(length-offset,buffer, offset);
					xMsg.Unmarshal(sbuf,parseMessage);
					return true;
				}
				else if ( parseMessage.Selector == 0 && parseMessage.Source.EqualsNetwork(mOwnAddress))
					Deliver(parseMessage.Sequence);
				else
					mHandleUnknownMessage(AppName+".XtedsCmd", parseMessage);
			}
			else // ProtocolId.XtedsData
			{
				XComponent comp = FindComponent(parseMessage.Source);
				if (comp != null || parseMessage.HaveSecondaryHeaders)
				{
					XtedsMessage xMsg;
					if ( comp != null )
						xMsg = comp.Xteds.FindMessage(parseMessage.Selector);
					else
						xMsg = mXteds.FindMessage(parseMessage.Selector);
					if (xMsg != null)
					{
						if (parseMessage.HaveSecondaryHeaders)
						{
							mSecHdrIter.SetHeader(parseMessage);
							// If there is a Continuation, add the packet to the read buffer
							// Don't process the message until it is completely transmitted.
							if (mSecHdrIter.Has(SecondaryHeader.SecondaryHdrType.Continuation))
							{
								var c = Continuation.FromMessage(xMsg);
								mContHdr.Version = (byte)ProtocolId.XtedsData;
								if ( c.IsTransmitter )
								{
									int startNextPacket;
									int nextLen = c.NeedToTransmit(mSecHdrIter, out startNextPacket);
									if ( nextLen > 0 )
									{
										mContHdr.Source = parseMessage.Destination;
										base.Send(mContHdr,parseMessage.Selector, buffer, startNextPacket,
											nextLen, parseMessage.Source, parseMessage.Sequence);
									}
									return false;
								}
								else if ( mSecHdrIter.GetSecondaryHeader(c) != null )
								{
									int ackLen = c.AddPacket(buffer,offset,length-offset, mAckBuf);
									if ( ackLen > 0 ) // all subsequent packets
										mTransport.Write(mAckBuf,0,ackLen+mContHdr.Size,parseMessage.Source);
									else // first packet
									{
										Send(mContHdr, parseMessage.Selector, mAckBuf, mContHdr.Size, -ackLen, parseMessage.Source, parseMessage.Sequence);
									}
									if ( !c.Finished )
										return false;
									// Setup for actual message delivery
									buffer = c.PayloadBuffer;
									length = (int)c.PayloadLength;
								}
							}
						}
						if (xMsg.Disposition != XtedsMessage.DispositionType.Deliver)
						{
							if (xMsg.Disposition == XtedsMessage.DispositionType.Defer)
								deferredMessages.Add(new DeferredMsg(xMsg, buffer, offset, length));
							return false;
						}
						if (xMsg.IsSynchronous)
						{
							XtedsMessage request = xMsg.RequestMessage;
							if (request != null ||
								parseMessage.Sequence != request.Header.Sequence ||
								!(parseMessage.Source == request.Provider)
								)
							{
								mHandleUnknownMessage(AppName+".synchronous XtedsData", parseMessage);
								return false;
							}
						}
						sbuf.Set(length-offset, buffer, offset);
						xMsg.Unmarshal(sbuf, parseMessage);
						return true;
					}
					else
					{
						mHandleUnknownMessage(AppName+".XtedsData", parseMessage);
						return false;
					}
				}
				else if (mHandleUnknownComponent != null)
				{
					sbuf.Set(length-offset, buffer, offset);
					mHandleUnknownComponent(parseMessage, sbuf);
				}
				else
					mHandleUnknownMessage(AppName+".XtedsData", parseMessage);
			}
			return false;
		}

		public int Send(DeferredMsg deferredMessage)
		{
			var xMessage = deferredMessage.xtedsMessage;
			Message header = xMessage.Header;
			header.Source.WithRespectTo(deferredMessage.destination);
			int n = base.Send(header, xMessage.MessageId.Hash(), deferredMessage.buffer, deferredMessage.payloadOffset,
				(int)deferredMessage.payloadLength,deferredMessage.destination,deferredMessage.sequence);
			if ( header.HaveSecondaryHeaders ) header.PopHeaderSize();
			//delete deferredMessage;
			return n;
		}

		const int SizeofBuffer = 64*1024;
		byte[] sendBuffer = new byte[SizeofBuffer];

		public int Send(XtedsMessage xMessage, Address destination, int sequence=0)
		{
			if (xMessage == null) return 0;
			if (!xMessage.HeaderIsSet)
			{
				Message hdr = ThisThreadsHeader(this);
				//mSendMessage;
				xMessage.Header = hdr;
			}
			var header = xMessage.Header;
			uint bufferSize, payloadLength;
			int fixedHdrSize = header.Size, payloadOffset = fixedHdrSize;
			Continuation continuation;
		Again:
			var secHdr = xMessage.SecondaryHeaders;
			if ( secHdr != null )
			{
				byte[] buf = sendBuffer;
				int offset = fixedHdrSize;
				bufferSize = (uint)(SizeofBuffer-fixedHdrSize);

				// If we have a Continuation, Initialize it
				continuation = null;
				SecondaryHeader sh = xMessage.GetSecondaryHeader((int)SecondaryHeader.SecondaryHdrType.Continuation);
				if ( sh != null )
				{
					continuation = sh as Continuation;
					offset = (int)continuation.Initialize(fixedHdrSize, destination);
					buf = continuation.PayloadBuffer;
					bufferSize = continuation.Size - (uint)fixedHdrSize;
				}

				// Marshal all the secondary headers
				payloadOffset = offset;
				offset = xMessage.MarshalSecondaryHeaders(buf,offset);
				int secHdrLength = offset-payloadOffset, m = secHdrLength%4;
				if (m != 0) // force secondary headers to 4 byte quanta
				{
					buf[offset] = 0;
					offset += 4-m;
					secHdrLength = offset - payloadOffset;
				}
				header.PushHeaderSize((byte)((fixedHdrSize+secHdrLength)>>2));
				payloadLength = (uint)(xMessage.Marshal(buf, offset,(uint)(bufferSize-secHdrLength)) + secHdrLength);
				// If the marshaled length would exceed the buffer capacity, adjust the 
				// write buffer, either by adding a Continuation or resizing an existing one
				if (payloadLength > bufferSize)
				{
					if (continuation!=null)
						continuation.Resize(payloadLength);
					else
						xMessage.AddSecondaryHeader(new Continuation(payloadLength));
					goto Again;
				}
				// Need to always set payload length after marshaling so subsequent messages
				// always send the right number of packets.
				continuation.SetPayloadLength(payloadLength);
				buf = continuation.PayloadBuffer;
				for (sh = secHdr; sh != null; sh = sh.Next)
					offset += sh.Marshal(buf,offset);

				if (continuation != null) // Adjust the written length for the first packet
					payloadLength = continuation.SendLength(secHdrLength);
			}
			else
			{
				payloadOffset = fixedHdrSize;
				payloadLength = (uint)xMessage.Marshal(sendBuffer, payloadOffset, (uint)(SizeofBuffer - fixedHdrSize));
				// If the marshaled length would exceed the buffer capacity, adjust the 
				// write buffer by adding a Continuation
				if (payloadLength > SizeofBuffer)
				{
					xMessage.AddSecondaryHeader(new Continuation(payloadLength));
					goto Again;
				}
			}
			if (xMessage is IDataMessage || xMessage.ReplyMessage == xMessage)
				header.Version = (byte)ProtocolId.XtedsData;
			else
				header.Version = (byte)ProtocolId.XtedsCommand;
			if ( destination == null )
				destination = xMessage.Provider;
			if ( destination == null )
			{
				if ( xMessage is IDataMessage )
					Logger.Log(1,"{0} is a Notification data message;get it as a DataMessage from OwnDataMessage() and Publish(msg) instead of Send() \n",
						xMessage.Name);
				else
					Logger.Log(1, "Can't send {0} to unknown destination.\n", xMessage.Name);
				return 0;
			}

			header.Source.WithRespectTo(destination);
			int n = base.Send(header,xMessage.MessageId.Hash(), sendBuffer, payloadOffset, (int)payloadLength,
				destination, sequence);
			if (secHdr != null) header.PopHeaderSize();
			return n;
		}

		//public int SendMultiple(IDataMessage message, AddressList destinations)
		//{
		//	if (message == null) return 0;
		//	int payload = 24;
		//	int length = message.XtedsMessage.Marshal(sendBuffer, payload, SizeofBuffer);
		//	mSendMessage.Version = (byte)ProtocolId.XtedsData;
		//	return base.SendMultiple(message.MessageId.Hash(), sendBuffer, payload, length,
		//		destinations);
		//}

		private Message ThisThreadsHeader(XtedsProtocol xtedsProtocol)
		{
			var thread = Thread.CurrentThread;
			for(int i=0;i<headerByThread.Count; i++)
				if ( headerByThread[i].thread == thread)
					return headerByThread[i].header;

			Message hdr = xtedsProtocol.CloneHeader();
			headerByThread.Add(new ThreadHeader(thread,hdr));
			return hdr;
		}

		public void WhenUnknownComponent(UnknownComponentHandler handleUnknownComponent)
		{
			mHandleUnknownComponent = handleUnknownComponent;
		}

		public IXteds Xteds
		{
			set { mXteds = value; }
		} IXteds mXteds;
	}

	public class DeferredMsg : IDisposable
	{
		internal DeferredMsg() { }

		internal DeferredMsg(DeferredMsg src)
		{
			destination = src.destination;
			xtedsMessage = src.xtedsMessage;
			length = src.length;
			sequence = src.sequence;
			payloadLength = src.payloadLength;

			buffer = new byte[length];
			Buffer.BlockCopy(buffer, 0, this.buffer, 0, (int)length);
			payloadOffset = src.payloadOffset;
			src.buffer = null; // So the subsequent ~DeferredMsg won't delete it
		}

		internal DeferredMsg(XtedsMessage xtedsMessage, byte[] buffer, int offset, int length)
		{
			this.xtedsMessage = xtedsMessage;
			this.length = (uint)length;
			buffer = new byte[length];
			Buffer.BlockCopy(buffer, offset, this.buffer, 0, length);
		}

		internal void Set(XtedsMessage xtedsMessage, byte[] buffer, int length, Address destination, int sequence)
		{
			this.destination = destination;
			this.xtedsMessage = xtedsMessage;
			// Note: don't make a copy of the buffer
			this.buffer = buffer;
			this.length = (uint)length;
			this.sequence = sequence;
			payloadOffset = 0;
			payloadLength = 0;
		}

		internal Address destination;
		internal XtedsMessage xtedsMessage;
		internal byte[] buffer;
		internal int payloadOffset, sequence;
		internal uint length, payloadLength;

		public void Dispose()
		{
			buffer = null;
		}
	}

	public class XComponent : IHostXtedsLite
	{
		Address mAddress, mDomainAddress;

		public XComponent(Address address, Address domainAddress)
		{
			mAddress = address;
			//mXteds.Host = this;
			mXteds = IXteds.Parse(this, null, null);
			mDomainAddress = domainAddress.Clone();
		}
		public Address Address { get { return mAddress; } internal set { mAddress = value; } }
		public IXteds Xteds { get { return mXteds; } internal set { mXteds = value; } }
		IXteds mXteds;
		public HostType HostType { get { return HostType.Consumer; } }
		public int AllowableLeasePeriod
		{
			get { return 0; }
		}
	}

	internal class ThreadHeader
	{
		internal Thread thread;
		internal Message header;
		internal ThreadHeader(Thread thread, Message hdr)
		{
			this.thread = thread;
			header = hdr;
		}
	};

}
