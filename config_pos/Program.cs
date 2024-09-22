﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace config_pos
{
    internal static class Program
    {

        public static Dictionary<string, string> imagesBase64 = new Dictionary<string, string>
        {
            { "behpardakht", "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" },
            { "parsiyan", "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAfCAYAAABXscv8AAAAAXNSR0IB2cksfwAAAAlwSFlzAAAOxAAADsQBlSsOGwAABM9JREFUeJztWGtMU3cU5wIt9EHLo0ChczP4mPtQMhuMiXGzupGMYqGFWkQeghIT7F4x48NM3MyiS8zkoaFPirI5x9y+LdtiYphfBnVrxgfa6QdjfKT6ZZvobUfmFNjvT3pn3y3zto2zJ/nl9t57/uee8zvn/P8HsrIykpGnSniDbjmn31XHHXDV5g24lMAm/pC7puCEW5pu35IuwiH36qxPZh4Bi6Gg8Jw/6K5Ot49Jldx+19lIwTNANejT7WPSRHTCXYksP4hDQEu6/UyaIPsHYgXPEPDBzJ8UQbr9ZVWkpsvC7OMz7kQI2HvJu7Z9ij7f5aA39U37/h9EcAZc9QhwPh4BqBJCQHXbJL0IPAARE51TdPObTm9uumN4IsHRtyLnuGsiHgE5wQQweNg+SV8AEfr9TvrpJgLHnAat4FgmAf+iY5K+shsVse8nb17ED0ha7IWc7aYjVd1neGw5XagbEXPUpr7SnaMCtmyiImpQ7udC2yIeAQxQETf2XKJlQUb5Gktptsp4M6tueBEQseGoqNn2IqUangU8IEDChs1A4Q26NiJoO47HWf8wlBABBN0OekOQsbwG8yo4Os8yAUpij1IZ78g6xsrZsBlJBENuOSEC7aFKsAKuv/OLL3g/yG+0gADjQnIIGPaU7zpVzIbNWCIfu0olQkDHFH04bDEIkMHROeKwWGcrZMMhboNZ5Sfgt4ImaxUbNuNJTxwCkP27mBHWhC0sax3lYg+4TBzGRnhQqLW9nlNv3Ip75XLAVZu3FTTZatFSmux6o8NfUQv4/R3eqXgay6uAEt9Y0uc1Wl4j+gR8jbUWiahl7hnA1rbAb4BQJZ5tXNvzediggx1eHif7J/c7vZEHJK7a1Ot3mE0sJMEmqarb0l2n+KEx7HHQEgR5LQoBc10OrzxmCeU3mltRCTee2EEEjgpyIdub0Qq9qIBbLBPgqWw/HfW4fm/aJzA4fRKD01uOqpACL6A9ymIGHyjPd47xlzbDZaJEbxe/bPiyeN2+s0HO9QxNUMiY8L/YjATMF6zNFQmLQGutQI8qkNVXUCkbhFpr2crdn8b9g0PcbOPnN5hfwqmggI0lFO0YURTr7SK+1iqFzdWhwDdWFentQvR6NVpTwQD3CswslamIN0zw8XdDyvAepryP5b3jUUmoefsrCq3wLXT/RmvNUY/hE2gt67D+DN7fD0XudtOvwibrerTO70T/8dphcp2FL6n/54dYN8KDcx44Mw8nmKFpAc60RlsjaRl9Dk57UTFvoBJkyP4SEFwFeV/aOioqabGXh0Ky016I4zgbp0AF0Uf1yPBbhlNDBnIO4/tXV3SOcVIXvV/giBSOVOHIWoPsTBESQMpINP3KttN5ZFOF7gxwEY5fhP4PgEmksxXh+iHefx8KPP8ahK/E0bkV92qsU+OZGjYacT2HZ9NV3Z/lpDL2MIEj434CxmPpkb6FziHoHiFARRwlxxgy+T4C+YbMC3h/IBCYE9qwx2A4M9I4Uf7AOg8D6P+MitqSqjijCpz/ghCQi4zE0gMBdQjWwADBvUX+8MLvj/wE/Bj43q9jwGa3GUT9hasiVTEtS5CpThBwEteuaDq6o+cpZPEY0QsEAutHFksYG5EA4raAjD6M0ak/8jKSkYw8E/IPaeZAAFi2XD8AAAAASUVORK5CYII=" },
            { "samankish", "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAfCAYAAABXscv8AAAAAXNSR0IB2cksfwAAAAlwSFlzAAAOxAAADsQBlSsOGwAABM9JREFUeJztWGtMU3cU5wIt9EHLo0ChczP4mPtQMhuMiXGzupGMYqGFWkQeghIT7F4x48NM3MyiS8zkoaFPirI5x9y+LdtiYphfBnVrxgfa6QdjfKT6ZZvobUfmFNjvT3pn3y3zto2zJ/nl9t57/uee8zvn/P8HsrIykpGnSniDbjmn31XHHXDV5g24lMAm/pC7puCEW5pu35IuwiH36qxPZh4Bi6Gg8Jw/6K5Ot49Jldx+19lIwTNANejT7WPSRHTCXYksP4hDQEu6/UyaIPsHYgXPEPDBzJ8UQbr9ZVWkpsvC7OMz7kQI2HvJu7Z9ij7f5aA39U37/h9EcAZc9QhwPh4BqBJCQHXbJL0IPAARE51TdPObTm9uumN4IsHRtyLnuGsiHgE5wQQweNg+SV8AEfr9TvrpJgLHnAat4FgmAf+iY5K+shsVse8nb17ED0ha7IWc7aYjVd1neGw5XagbEXPUpr7SnaMCtmyiImpQ7udC2yIeAQxQETf2XKJlQUb5Gktptsp4M6tueBEQseGoqNn2IqUangU8IEDChs1A4Q26NiJoO47HWf8wlBABBN0OekOQsbwG8yo4Os8yAUpij1IZ78g6xsrZsBlJBENuOSEC7aFKsAKuv/OLL3g/yG+0gADjQnIIGPaU7zpVzIbNWCIfu0olQkDHFH04bDEIkMHROeKwWGcrZMMhboNZ5Sfgt4ImaxUbNuNJTxwCkP27mBHWhC0sax3lYg+4TBzGRnhQqLW9nlNv3Ip75XLAVZu3FTTZatFSmux6o8NfUQv4/R3eqXgay6uAEt9Y0uc1Wl4j+gR8jbUWiahl7hnA1rbAb4BQJZ5tXNvzediggx1eHif7J/c7vZEHJK7a1Ot3mE0sJMEmqarb0l2n+KEx7HHQEgR5LQoBc10OrzxmCeU3mltRCTee2EEEjgpyIdub0Qq9qIBbLBPgqWw/HfW4fm/aJzA4fRKD01uOqpACL6A9ymIGHyjPd47xlzbDZaJEbxe/bPiyeN2+s0HO9QxNUMiY8L/YjATMF6zNFQmLQGutQI8qkNVXUCkbhFpr2crdn8b9g0PcbOPnN5hfwqmggI0lFO0YURTr7SK+1iqFzdWhwDdWFentQvR6NVpTwQD3CswslamIN0zw8XdDyvAepryP5b3jUUmoefsrCq3wLXT/RmvNUY/hE2gt67D+DN7fD0XudtOvwibrerTO70T/8dphcp2FL6n/54dYN8KDcx44Mw8nmKFpAc60RlsjaRl9Dk57UTFvoBJkyP4SEFwFeV/aOioqabGXh0Ky016I4zgbp0AF0Uf1yPBbhlNDBnIO4/tXV3SOcVIXvV/giBSOVOHIWoPsTBESQMpINP3KttN5ZFOF7gxwEY5fhP4PgEmksxXh+iHefx8KPP8ahK/E0bkV92qsU+OZGjYacT2HZ9NV3Z/lpDL2MIEj434CxmPpkb6FziHoHiFARRwlxxgy+T4C+YbMC3h/IBCYE9qwx2A4M9I4Uf7AOg8D6P+MitqSqjijCpz/ghCQi4zE0gMBdQjWwADBvUX+8MLvj/wE/Bj43q9jwGa3GUT9hasiVTEtS5CpThBwEteuaDq6o+cpZPEY0QsEAutHFksYG5EA4raAjD6M0ak/8jKSkYw8E/IPaeZAAFi2XD8AAAAASUVORK5CYII=" },
            { "omidpay", "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" }
        };
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_configpos());
        }
    }
    public class DateConverter
    {

        public static string ConvertToPersianDate(string dateString)
        {
            dateString = dateString.Trim();
            DateTime gregorianDate;
            try
            {
                gregorianDate = DateTime.Parse(dateString);
            }
            catch (Exception)
            {
                throw new ArgumentException("INVALIDDATE");
            }
            PersianCalendar persianCalendar = new PersianCalendar();
            int persianYear = persianCalendar.GetYear(gregorianDate);
            int persianMonth = persianCalendar.GetMonth(gregorianDate);
            int persianDay = persianCalendar.GetDayOfMonth(gregorianDate);
            return $"{persianYear:D4}{persianMonth:D2}{persianDay:D2}";
        }
        public static string ConvertToFormattedTime(string timeString)
        {
            // تلاش برای پارس کردن رشته زمان
            try
            {
                // جدا کردن ساعت، دقیقه و ثانیه
                string[] timeParts = timeString.Split(':');

                if (timeParts.Length != 3)
                {
                    throw new ArgumentException("INVALIDTIMEFORMAT");
                }

                // جدا کردن ثانیه و اعشار ثانیه
                string[] secondParts = timeParts[2].Split('.');

                // اگر ثانیه ها عدد اعشاری ندارند، فقط ثانیه را برمی‌داریم
                string seconds = secondParts[0];

                // قالب‌بندی و بازگرداندن ساعت، دقیقه و ثانیه به صورت دو رقمی
                return $"{int.Parse(timeParts[0]):D2}:{int.Parse(timeParts[1]):D2}:{int.Parse(seconds):D2}";
            }
            catch (Exception)
            {
                throw new ArgumentException("INVALIDTIMEFORMAT");
            }
        }



    }
}
