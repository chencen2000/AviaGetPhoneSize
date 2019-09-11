using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AviaGetPhoneSize
{
    class LedController
    {
        SerialPort _serialPort = null;
        public LedController(string port)
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = port;
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout = 2000;
            _serialPort.NewLine = "\r\n";
        }
        void logit(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        public bool open()
        {
            try
            {
                DateTime _start = DateTime.Now;
                _serialPort.Open();
                //do
                //{
                //    System.Threading.Thread.Sleep(1000);
                //    if ((DateTime.Now - _start).TotalSeconds > 3)
                //        break;
                //} while (_serialPort.BytesToRead == 0);
                //string s = _serialPort.ReadLine();
            }
            catch (Exception) { }
            return _serialPort.IsOpen;
        }
        public void close()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception) { }
        }
        public bool turn_onoff()
        {
            bool ret = false;
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write("$");
                    int b = _serialPort.ReadByte();
                    if (b == '$')
                        ret = true;
                }
                catch (Exception) { }
            }
            logit($"turn_onoff: -- ret={ret}");
            return ret;
        }
        public bool switch_led()
        {
            bool ret = true;
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write(" ");
                    int b = _serialPort.ReadByte();
                    if (b == ' ')
                        ret = true;
                }
                catch (Exception) { }
            }
            logit($"switch_led: -- ret={ret}");
            return ret;
        }
        public bool level_up()
        {
            bool ret = true;
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write("+");
                    int b = _serialPort.ReadByte();
                    if (b == '+')
                        ret = true;
                }
                catch (Exception) { }
            }
            logit($"level_up: -- ret={ret}");
            return ret;
        }
        public bool level_down()
        {
            bool ret = true;
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write("-");
                    int b = _serialPort.ReadByte();
                    if (b == '-')
                        ret = true;
                }
                catch (Exception) { }
            }
            logit($"level_down: -- ret={ret}");
            return ret;
        }
        public bool level_go(int step)
        {
            bool ret = true;
            if (_serialPort.IsOpen)
            {
                string ws = step > 0 ? "+" : "-";
                for (int i = 0; i < Math.Abs(step); i++)
                {
                    try
                    {
                        _serialPort.DiscardInBuffer();
                        _serialPort.Write(ws);
                        int b = _serialPort.ReadByte();
                        System.Threading.Thread.Sleep(200);
                    }

                    catch (Exception) { }
                }
            }
            logit($"level_go: -- ret={ret}");
            return ret;
        }
        public bool level_set(int value)
        {
            bool ret = false;
            if (_serialPort.IsOpen && value>0)
            {
                while (!ret)
                {
                    Tuple<bool, int, int> led_value = read_value();
                    if (led_value.Item1)
                    {
                        int v = Math.Max(led_value.Item2, led_value.Item3);
                        if (v == value)
                            ret = true;
                        else
                        {
                            int step = (v - value) / 5;
                            string ws = step > 0 ? "+" : "-";
                            for (int i = 0; i < Math.Abs(step); i++)
                            {
                                try
                                {
                                    _serialPort.DiscardInBuffer();
                                    _serialPort.Write(ws);
                                    int b = _serialPort.ReadByte();
                                    System.Threading.Thread.Sleep(200);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            logit($"level_go: -- ret={ret}");
            return ret;
        }
        public Tuple<bool,int,int> read_value()
        {
            bool ret = false;
            int v1 = 0;
            int v2 = 0;
            Regex reg = new Regex(@"(\d*),(\d*)\r\n");
            DateTime _start = DateTime.Now;
            try
            {
                if (_serialPort.IsOpen)
                {
                    while (!ret)
                    {
                        StringBuilder sb = new StringBuilder();
                        _serialPort.DiscardInBuffer();
                        _serialPort.Write("?");
                        bool done = false;
                        while (!done)
                        {
                            //System.Threading.Thread.Sleep(2000);
                            if (_serialPort.BytesToRead > 0)
                            {
                                byte[] b = new byte[_serialPort.BytesToRead];
                                _serialPort.Read(b, 0, b.Length);
                                string s = System.Text.Encoding.ASCII.GetString(b);
                                //string s = _serialPort.ReadLine();
                                logit($"read_value: {s}");
                                sb.Append(s);
                            }
                            // check
                            if (sb.Length > 0)
                            {
                                Match m = reg.Match(sb.ToString());
                                if (m.Success)
                                {
                                    v1 = Int32.Parse(m.Groups[1].Value);
                                    v2 = Int32.Parse(m.Groups[2].Value);
                                    done = true;
                                    //if (v1 % 5 == 0 && v2 % 5 == 0)
                                    //    ret = true;
                                    double d = 1.0 * v1 / 5;
                                    d = Math.Round(d, MidpointRounding.AwayFromZero);
                                    v1 = Convert.ToInt32(d * 5);
                                    d = 1.0 * v2 / 5;
                                    d = Math.Round(d, MidpointRounding.AwayFromZero);
                                    v2 = Convert.ToInt32(d * 5);
                                    ret = true;
                                }
                            }
                        }
                        if ((DateTime.Now - _start).TotalSeconds > 10)
                            break;
                    }
                }
            }
            catch (Exception) { }
            logit($"read_value: ret={ret}, v1={v1}, v2={v2}");
            return new Tuple<bool, int, int>(ret,v1,v2);
        }

        #region
        public bool power_on()
        {
            bool ret = false;
            logit("power_on: ++");
            while (!ret)
            {
                Tuple<bool, int, int> led_value = read_value();
                if (led_value.Item1)
                {
                    if (led_value.Item3 == 0 && led_value.Item2 == 0)
                    {
                        // power offed
                        turn_onoff();
                        // 3 time '-'
                        level_down();
                        level_down();
                        level_down();
                    }
                    else
                    {
                        ret = true;
                    }
                }
            }
            // 
            logit("power_on: --");
            return ret;
        }
        public bool turn_on_both_led()
        {
            bool ret = false;
            logit("turn_on_both_led: ++");
            while (!ret)
            {
                Tuple<bool, int, int> led_value = read_value();
                if (led_value.Item1)
                {
                    if (led_value.Item3 > 0 && led_value.Item2 > 0)
                    {
                        // both led is on
                        ret = true;
                    }
                    else
                    {
                        switch_led();
                    }                        
                }
            }
            // 
            logit("turn_on_both_led: --");
            return ret;
        }
        public bool turn_on_cold_led()
        {
            bool ret = false;
            logit("turn_on_cold_led: ++");
            while (!ret)
            {
                Tuple<bool, int, int> led_value = read_value();
                if (led_value.Item1)
                {
                    if (led_value.Item3 == 0 && led_value.Item2 > 0)
                    {
                        // both led is on
                        ret = true;
                    }
                    else
                    {
                        switch_led();
                    }
                }
            }
            // 
            logit("turn_on_cold_led: --");
            return ret;
        }
        public bool turn_on_warm_led()
        {
            bool ret = false;
            logit("turn_on_warm_led: ++");
            while (!ret)
            {
                Tuple<bool, int, int> led_value = read_value();
                if (led_value.Item1)
                {
                    if (led_value.Item3 > 0 && led_value.Item2 == 0)
                    {
                        // both led is on
                        ret = true;
                    }
                    else
                    {
                        switch_led();
                    }
                }
            }
            // 
            logit("turn_on_warm_led: --");
            return ret;
        }
        public bool set_led_to_detect_size()
        {
            bool ret = false;
            logit("set_led_to_detect_size: ++");
            power_on();
            turn_on_warm_led();
            level_set(35);
            logit("set_led_to_detect_size: --");
            return ret;
        }
        #endregion
    }
}
