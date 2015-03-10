using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MuiltClientTest.VM
{
   public class baseCmd:ICommand
    {

       public baseCmd()
       { 
       
       }

       Action _a;
       public baseCmd(Action a)
       {
           _a = a;
       }

      public  event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
          return true;
      }

      public virtual void Execute(object parameter)
      {
          _a();
      }
    }
}
