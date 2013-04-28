﻿#define SYMBOL_RATE_ROUNDING

using System.Globalization;
using ChanSort.Api;

namespace ChanSort.Loader.LG
{
  internal class SatTransponder : Transponder
  {
    private const string _FirstChannelIndex = "offFirstChannelIndex";
    private const string _LastChannelIndex = "offLastChannelIndex";
    private const string _ChannelCount = "offChannelCount";
    private const string _Frequency = "offFrequency";
    private const string _OriginalNetworkId = "offOriginalNetworkId";
    private const string _TransportStreamId = "offTransportStreamId";
    private const string _SymbolRate = "offSymbolRate";
    private const string _SatIndex = "offSatIndex";

    private readonly DataMapping mapping;
    private readonly byte[] data;
    private readonly int offset;
    private int symbolRate;
    private int firstChannelIndex;
    private int lastChannelIndex;

    public SatTransponder(int index, DataMapping mapping, DataRoot dataRoot) : base(index)
    {
      this.mapping = mapping;
      this.data = mapping.Data;
      this.offset = mapping.BaseOffset;

      this.firstChannelIndex = mapping.GetWord(_FirstChannelIndex);
      this.lastChannelIndex = mapping.GetWord(_LastChannelIndex);

      this.FrequencyInMhz = mapping.GetWord(_Frequency);
      this.OriginalNetworkId = mapping.GetWord(_OriginalNetworkId);
      this.TransportStreamId = mapping.GetWord(_TransportStreamId);
      this.symbolRate = mapping.GetWord(_SymbolRate) & 0x7FFF;

#if SYMBOL_RATE_ROUNDING
      if (this.symbolRate%100 >= 95)
      {
        this.symbolRate = (this.symbolRate/100 + 1)*100;
        mapping.SetWord(_SymbolRate, (mapping.GetWord(_SymbolRate) & 0x8000) + this.symbolRate);
      }
#endif

      string strFactor = mapping.Settings.GetString("symbolRateFactor");
      decimal factor;
      if (!string.IsNullOrEmpty(strFactor) && decimal.TryParse(strFactor, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out factor))
        this.symbolRate = (int)(this.symbolRate * factor);
      this.Satellite = dataRoot.Satellites.TryGet(mapping.GetByte(_SatIndex)/2);
    }

    public int FirstChannelIndex
    {
      get { return this.firstChannelIndex; }
      set
      {
        mapping.SetDataPtr(this.data, this.offset);
        mapping.SetWord(_FirstChannelIndex, value);
        this.firstChannelIndex = value;
      }
    }

    public int LastChannelIndex
    {
      get { return lastChannelIndex; }
      set
      {
        mapping.SetDataPtr(this.data, this.offset);
        mapping.SetWord(_LastChannelIndex, value);
        this.lastChannelIndex = value;
      }
    }

    public int ChannelCount
    {
      set
      {
        mapping.SetDataPtr(this.data, this.offset);
        mapping.SetWord(_ChannelCount, value);
      }      
    }

    public override int SymbolRate
    {
      get { return symbolRate; }
      set
      {
        mapping.SetDataPtr(this.data, this.offset);
        mapping.SetWord(_SymbolRate, value);
        this.symbolRate = value;
      }
    }
  }
}
