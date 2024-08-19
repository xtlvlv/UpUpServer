namespace UpUpServer;

public static class TimeUtil
{
    private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    // 精确毫秒 1677401019571.67
    public static double GetMillTimeStamp()
    {
        TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return timeSpan.TotalMilliseconds;
    }
    
    // 毫秒 1677372219570
    public static long NowMs()
    {
        return (DateTime.UtcNow.Ticks - epoch) / 10000;
    }

    // 秒 1677372219
    public static long NowSeconds()
    {
        return (DateTime.UtcNow.Ticks - epoch) / 10000000;
    }
}

/*
获得当前系统时间： DateTime dt = DateTime.Now;
Environment.TickCount可以得到“系统启动到现在”的毫秒值
DateTime now = DateTime.Now;
Console.WriteLine(now.ToString("yyyy-MM-dd"));  //按yyyy-MM-dd格式输出s
Console.WriteLine(dt.ToString());    //  26/11/2009 AM 11:21:30
Console.WriteLine(dt.ToFileTime().ToString());   //   129036792908014024
// Converts the value of the current System.DateTime object to a Windows file time
Console.WriteLine(dt.ToFileTimeUtc().ToString());  //     129036792908014024
// Converts the value of the current System.DateTime object to a Windows file time
Console.WriteLine(dt.ToLocalTime().ToString());   //       26/11/2009 AM 11:21:30
// Converts the value of the current System.DateTime object to local time.
Console.WriteLine(dt.ToLongDateString().ToString());   //      2009年11月26日
Console.WriteLine(dt.ToLongTimeString().ToString());  //      AM 11:21:30
Console.WriteLine(dt.ToOADate().ToString());   //      40143.4732731597
Console.WriteLine(dt.ToShortDateString().ToString());   //     26/11/2009
Console.WriteLine(dt.ToShortTimeString().ToString());   //     AM 11:21
Console.WriteLine(dt.ToUniversalTime().ToString());   //       26/11/2009 AM 3:21:30
Console.WriteLine(dt.Year.ToString());   //        2009
Console.WriteLine(dt.Date.ToString());   //        26/11/2009 AM 12:00:00
Console.WriteLine(dt.DayOfWeek.ToString());  //       Thursday
Console.WriteLine(dt.DayOfYear.ToString());   //       330
Console.WriteLine(dt.Hour.ToString());       //        11
Console.WriteLine(dt.Millisecond.ToString());   //     801        （毫秒）
Console.WriteLine(dt.Minute.ToString());   //      21
Console.WriteLine(dt.Month.ToString());   //       11
Console.WriteLine(dt.Second.ToString());   //      30
Console.WriteLine(dt.Ticks.ToString());   //       633948312908014024
 
Console.WriteLine(dt.TimeOfDay.ToString());   //       12:29:51.5181524
// Gets the time of day for this instance.
// 返回 A System.TimeSpan that represents the fraction of the day that has elapsed since midnight.
Console.WriteLine(dt.ToString());     //     26/11/2009 PM 12:29:51
Console.WriteLine(dt.AddYears(1).ToString());    //         26/11/2010 PM 12:29:51
Console.WriteLine(dt.AddDays(1.1).ToString());    //        27/11/2009 PM 2:53:51
Console.WriteLine(dt.AddHours(1.1).ToString());    //       26/11/2009 PM 1:35:51
Console.WriteLine(dt.AddMilliseconds(1.1).ToString());    //26/11/2009 PM 12:29:51
Console.WriteLine(dt.AddMonths(1).ToString());    //        26/12/2009 PM 12:29:51
Console.WriteLine(dt.AddSeconds(1.1).ToString());    //     26/11/2009 PM 12:29:52
Console.WriteLine(dt.AddMinutes(1.1).ToString());    //     26/11/2009 PM 12:30:57
Console.WriteLine(dt.AddTicks(1000).ToString());    //      26/11/2009 PM 12:29:51
Console.WriteLine(dt.CompareTo(dt).ToString());    //       0
Console.WriteLine(dt.Add(new TimeSpan(1,0,0,0)).ToString());    // 加上一个时间段
（注：
System.TimeSpan为一个时间段，构造函数如下
public TimeSpan(long ticks); // ticks: A time period expressed in 100-nanosecond units.
                           //nanosecond：十亿分之一秒   new TimeSpan(10,000,000)        为一秒
public TimeSpan(int hours, int minutes, int seconds);
public TimeSpan(int days, int hours, int minutes, int seconds);
public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds);
）
Console.WriteLine(dt.Equals("2005-11-6 16:11:04").ToString());     //        False
Console.WriteLine(dt.Equals(dt).ToString());    //      True
Console.WriteLine(dt.GetHashCode().ToString());    //       1103291775
Console.WriteLine(dt.GetType().ToString());    //       System.DateTime
Console.WriteLine(dt.GetTypeCode().ToString());    //       DateTime
  
long Start = Environment.TickCount;   //单位是毫秒
long End = Environment.TickCount;
Console.WriteLine("Start is : "+Start);
Console.WriteLine("End is : "+End);
Console.WriteLine("The Time is {0}",End-Start);
Console.WriteLine(dt.GetDateTimeFormats('s')[0].ToString());    //2009-11-26T13:29:06
Console.WriteLine(dt.GetDateTimeFormats('t')[0].ToString());    //PM 1:29
Console.WriteLine(dt.GetDateTimeFormats('y')[0].ToString());    //2009年11月
Console.WriteLine(dt.GetDateTimeFormats('D')[0].ToString());    //2009年11月26日
Console.WriteLine(dt.GetDateTimeFormats('D')[1].ToString());    //星期四, 26 十一月, 2009
Console.WriteLine(dt.GetDateTimeFormats('D')[2].ToString());    //26 十一月, 2009
Console.WriteLine(dt.GetDateTimeFormats('D')[3].ToString());    //星期四 2009 11 26
Console.WriteLine(dt.GetDateTimeFormats('M')[0].ToString());    //26 十一月
Console.WriteLine(dt.GetDateTimeFormats('f')[0].ToString());    //2009年11月26日 PM 1:29
Console.WriteLine(dt.GetDateTimeFormats('g')[0].ToString());    //26/11/2009 PM 1:29
Console.WriteLine(dt.GetDateTimeFormats('r')[0].ToString());    //Thu, 26 Nov 2009 13:29:06 GMT
(注：
常用的日期时间格式：
格式 说明      输出格式 
d 精简日期格式 MM/dd/yyyy 
D 详细日期格式 dddd, MMMM dd, yyyy 
f  完整格式    (long date + short time) dddd, MMMM dd, yyyy HH:mm 
F 完整日期时间格式 (long date + long time) dddd, MMMM dd, yyyy HH:mm:ss 
g 一般格式 (short date + short time) MM/dd/yyyy HH:mm 
G 一般格式 (short date + long time) MM/dd/yyyy HH:mm:ss 
m,M 月日格式 MMMM dd 
s 适中日期时间格式 yyyy-MM-dd HH:mm:ss 
t 精简时间格式 HH:mm 
T 详细时间格式 HH:mm:ss
)
 
Console.WriteLine(string.Format("{0:d}", dt));    //28/12/2009
Console.WriteLine(string.Format("{0:D}", dt));    //2009年12月28日
Console.WriteLine(string.Format("{0:f}", dt));    //2009年12月28日 AM 10:29
Console.WriteLine(string.Format("{0:F}", dt));    //2009年12月28日 AM 10:29:18
Console.WriteLine(string.Format("{0:g}", dt));    //28/12/2009 AM 10:29
Console.WriteLine(string.Format("{0:G}", dt));    //28/12/2009 AM 10:29:18
Console.WriteLine(string.Format("{0:M}", dt));    //28 十二月
Console.WriteLine(string.Format("{0:R}", dt));    //Mon, 28 Dec 2009 10:29:18 GMT
Console.WriteLine(string.Format("{0:s}", dt));    //2009-12-28T10:29:18
Console.WriteLine(string.Format("{0:t}", dt));    //AM 10:29
Console.WriteLine(string.Format("{0:T}", dt));    //AM 10:29:18
Console.WriteLine(string.Format("{0:u}", dt));    //2009-12-28 10:29:18Z
Console.WriteLine(string.Format("{0:U}", dt));    //2009年12月28日 AM 2:29:18
Console.WriteLine(string.Format("{0:Y}", dt));    //2009年12月
Console.WriteLine(string.Format("{0}", dt));    //28/12/2009 AM 10:29:18
Console.WriteLine(string.Format("{0:yyyyMMddHHmmssffff}", dt));    //200912281029182047
 
计算2个日期之间的天数差
DateTime dt1 = Convert.ToDateTime("2007-8-1");    
DateTime dt2 = Convert.ToDateTime("2007-8-15");   
TimeSpan span = dt2.Subtract(dt1);              
int dayDiff = span.Days ;                    
 
计算某年某月的天数
int days = DateTime.DaysInMonth(2009, 8);       
days = 31;                                      
 
给日期增加一天、减少一天
DateTime dt =DateTime.Now;
dt.AddDays(1); //增加一天 dt本身并不改变
dt.AddDays(-1);//减少一天 dt本身并不改变
*/