using System;
using System.Collections.Generic;

namespace greectl
{
    public static class Tools
    {
        public static void PrintBits(uint n)
        {
            for (int i = 31; i >= 0; i--)
                Console.Write("{0}", (n & ((uint)1 << i)) > 0 ? "1" : "0");
            Console.WriteLine();
        }
    }

    public enum GreeOnOff
    {
        Off, On
    };

    public enum GreeMode
    {
        Auto, Cool, Dry, Fan, Heat
    };

    public enum GreeFanSpeed
    {
        Auto, Low, Mid, High
    };

    public enum GreeLouverPosition
    {
        Off, Full, Top, MidTop, Mid, MidBottom, Bottom, Bottom3, Mid3 = 0x9, Top3 = 0xb
    };

    public enum GreeDisplayMode
    {
        Off, Setting, Indoor, Outdoor
    };

    [Serializable]
    public class GreeState
    {
        public GreeFanSpeed FanSpeed { get; set; }
        public GreeLouverPosition LouverPosition { get; set; }
        public uint Temperature { get; set; }
        public GreeOnOff Turbo { get; set; }
        public GreeOnOff Display { get; set; }
        public GreeOnOff HealthMode { get; set; }
        public GreeOnOff XFan { get; set; }
        public GreeDisplayMode DisplayMode { get; set; }
    }

    public class GreeCtrl
    {
        private GreeState[] _states = new GreeState[5];

        private static readonly uint[][] _timings = new uint[][]
        {
            new uint[] { 632, 527 },     // 0
            new uint[] { 632, 1606 },    // 1
            new uint[] { 632, 19816 },   // 2
            new uint[] { 8948, 4422 },   // S
        };

        public GreeCtrl(GreeState[] states = null)
        {
            if (states == null)
                states = new GreeState[4];

            for (int i = 0; i < states.Length; i++)
            {
                // Default states
                // S000100001001000000000110000010100102000000000000000000000000000011010   Auto
                // S100100001001000000000110000010100102000000000000000000000000000000110   Cool
                // S010110001001000000000110000010100102000000000000000000000000000010110   Dry
                // S110100001001000000000110000010100102000000000000000000000000000001110   Fan
                // S001100000011000000000110000010100102000000000000000000000000000001000   Heat

                if (states[i] == null)
                {
                    if ((GreeMode)i == GreeMode.Auto)
                        _states[i] = new GreeState
                        {
                            FanSpeed = GreeFanSpeed.Auto,
                            LouverPosition = GreeLouverPosition.Off,
                            Temperature = 25,
                            Turbo = GreeOnOff.Off,
                            Display = GreeOnOff.On,
                            HealthMode = GreeOnOff.On,
                            XFan = GreeOnOff.Off,
                            DisplayMode = GreeDisplayMode.Off
                        };
                    else if ((GreeMode)i == GreeMode.Cool)
                        _states[(int)GreeMode.Cool] = new GreeState
                        {
                            FanSpeed = GreeFanSpeed.Auto,
                            LouverPosition = GreeLouverPosition.Off,
                            Temperature = 25,
                            Turbo = GreeOnOff.Off,
                            Display = GreeOnOff.On,
                            HealthMode = GreeOnOff.On,
                            XFan = GreeOnOff.Off,
                            DisplayMode = GreeDisplayMode.Off
                        };
                    else if ((GreeMode)i == GreeMode.Dry)
                        _states[(int)GreeMode.Dry] = new GreeState
                        {
                            FanSpeed = GreeFanSpeed.Low,
                            LouverPosition = GreeLouverPosition.Off,
                            Temperature = 25,
                            Turbo = GreeOnOff.Off,
                            Display = GreeOnOff.On,
                            HealthMode = GreeOnOff.On,
                            XFan = GreeOnOff.Off,
                            DisplayMode = GreeDisplayMode.Off
                        };
                    else if ((GreeMode)i == GreeMode.Fan)
                        _states[(int)GreeMode.Fan] = new GreeState
                        {
                            FanSpeed = GreeFanSpeed.Auto,
                            LouverPosition = GreeLouverPosition.Off,
                            Temperature = 25,
                            Turbo = GreeOnOff.Off,
                            Display = GreeOnOff.On,
                            HealthMode = GreeOnOff.On,
                            XFan = GreeOnOff.Off,
                            DisplayMode = GreeDisplayMode.Off
                        };
                    else if ((GreeMode)i == GreeMode.Heat)
                        _states[(int)GreeMode.Heat] = new GreeState
                        {
                            FanSpeed = GreeFanSpeed.Auto,
                            LouverPosition = GreeLouverPosition.Off,
                            Temperature = 28,
                            Turbo = GreeOnOff.Off,
                            Display = GreeOnOff.On,
                            HealthMode = GreeOnOff.On,
                            XFan = GreeOnOff.Off,
                            DisplayMode = GreeDisplayMode.Off
                        };
                }
                else
                    _states[i] = states[i];
            }
        }

        private static uint ReverseBits(uint n)
        {
            n = ((n >> 1) & 0x55555555) | ((n & 0x55555555) << 1);
            n = ((n >> 2) & 0x33333333) | ((n & 0x33333333) << 2);
            n = ((n >> 4) & 0x0F0F0F0F) | ((n & 0x0F0F0F0F) << 4);
            n = ((n >> 8) & 0x00FF00FF) | ((n & 0x00FF00FF) << 8);
            n = (n >> 16) | (n << 16);

            return n;
        }

        public GreeMode Mode { get; set; }
        public GreeOnOff Power { get; set; }
        public GreeFanSpeed FanSpeed
        {
            get { return _states[(int)Mode].FanSpeed; }
            set { _states[(int)Mode].FanSpeed = value; }
        }
        public GreeLouverPosition LouverPosition
        {
            get { return _states[(int)Mode].LouverPosition; }
            set { _states[(int)Mode].LouverPosition = value; }
        }
        public uint Temperature
        {
            get { return _states[(int)Mode].Temperature; }
            set { _states[(int)Mode].Temperature = value; }
        }
        public GreeOnOff Turbo
        {
            get { return _states[(int)Mode].Turbo; }
            set { _states[(int)Mode].Turbo = value; }
        }
        public GreeOnOff Display
        {
            get { return _states[(int)Mode].Display; }
            set { _states[(int)Mode].Display = value; }
        }
        public GreeOnOff HealthMode
        {
            get { return _states[(int)Mode].HealthMode; }
            set
            {
                for (int i = 0; i < _states.Length; i++)
                    _states[i].HealthMode = value;
            }
        }
        public GreeOnOff XFan
        {
            get { return _states[(int)Mode].XFan; }
            set { _states[(int)Mode].XFan = value; }
        }
        public GreeDisplayMode DisplayMode
        {
            get { return _states[(int)Mode].DisplayMode; }
            set { _states[(int)Mode].DisplayMode = value; }
        }

        public uint[] Bits
        {
            get
            {
                var b = new uint[2];
                b[0] |= 0x50000000;

                b[0] |= (uint)Mode;
                b[0] |= (uint)Power << 0x3;
                b[0] |= (uint)FanSpeed << 0x4;

                if (LouverPosition == GreeLouverPosition.Full ||
                    LouverPosition == GreeLouverPosition.Bottom3 ||
                    LouverPosition == GreeLouverPosition.Mid3 ||
                    LouverPosition == GreeLouverPosition.Top3)
                    b[0] |= (uint)1 << 6;
                b[1] |= (uint)LouverPosition;

                b[0] |= (Temperature - 0x10) << 0x8;
                b[1] |= ((Temperature + (uint)Mode + 0x2) % 0x10) << 0x1c;

                b[0] |= (uint)Turbo << 0x14;
                b[0] |= (uint)Display << 0x15;

                if (Power == GreeOnOff.On)
                    b[0] |= (uint)HealthMode << 0x16;
                else
                    b[0] ^= 0x80000000;

                b[0] |= (uint)XFan << 0x17;
                b[1] |= (uint)DisplayMode << 0x8;

                b[0] = ReverseBits(b[0]);
                b[1] = ReverseBits(b[1]);

                return b;
            }
        }

        public uint[] Timings
        {
            get
            {
                var t = new List<uint>();
                t.AddRange(_timings[3]);

                var bits = Bits;

                for (int i = 31; i >= 0; i--)
                    t.AddRange(_timings[(bits[0] & ((uint)1 << i)) >> i]);

                t.AddRange(_timings[0]);
                t.AddRange(_timings[1]);
                t.AddRange(_timings[0]);
                t.AddRange(_timings[2]);

                for (int i = 31; i >= 0; i--)
                    t.AddRange(_timings[(bits[1] & ((uint)1 << i)) >> i]);

                t.AddRange(_timings[0]);

                return t.ToArray();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ctrl = new GreeCtrl(
                new GreeState[]
                {
                    null,
                    new GreeState
                    {
                        FanSpeed = GreeFanSpeed.Low,
                        LouverPosition = GreeLouverPosition.Top,
                        Temperature = 23,
                        Turbo = GreeOnOff.Off,
                        Display = GreeOnOff.On,
                        HealthMode = GreeOnOff.Off,
                        XFan = GreeOnOff.On,
                        DisplayMode = GreeDisplayMode.Off
                    },
                    null,
                    null,
                    null
                }
            );

            ctrl.Power = GreeOnOff.On;
            ctrl.Mode = GreeMode.Cool;

            var bits = ctrl.Bits;
            Tools.PrintBits(bits[0]);
            Tools.PrintBits(bits[1]);

            foreach (var t in ctrl.Timings)
                Console.Write("{0} ", t);

            Console.WriteLine("\n\nPress <any key> to continue.");
            Console.ReadKey(true);
        }
    }
}
