using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MuiltClientTest.VM
{
    public class ConertString: BaseIvalueConvert
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return value.ToString();
            }

        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            if (targetType.Equals(typeof(int)))
            {
                if (value == null)
                {
                    return 0;
                }
                else
                {
                    int returnValue = 0;
                    int.TryParse(value.ToString(), out returnValue);
                    return returnValue; 
                }
            
            }

            if (targetType.Equals(typeof(float)))
            {
                float returnValue = 0;
                float.TryParse(value.ToString(), out returnValue);
                return returnValue; 
            }

            return null;
        }
    }


   public class BaseIvalueConvert : IValueConverter
   {

       public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
       {
           return null;
       }

       public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
       {
           return null;
       }
   }
}
