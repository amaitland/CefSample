using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefBrowserWrapper.FormManager
{
    public class FormManagerBase
    {
        private object _locker;
        private List<Form1> _freeForms = new List<Form1>();
        private List<Form1> _busyForms = new List<Form1>();
        private int _maxForms;

        public FormManagerBase(int maxForms)
        {
            _locker = new object();
            _freeForms = new List<Form1>();
            _busyForms = new List<Form1>();
            _maxForms = maxForms;
        }
        async Task<Form1> GetFreeForm()
        {
            Form1 retVal = _freeForms[0];
            while (true)
            {
                lock (_locker)
                {
                    if (_freeForms.Count > 0)
                    {
                        _busyForms.Add(_freeForms[0]);
                        _freeForms.RemoveAt(0);
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }
            return retVal;
        }

        void ReleaseForm(Form1 form)
        {
            lock (_locker)
            {
                _busyForms.Remove(form);
            }
        }
    }
}
