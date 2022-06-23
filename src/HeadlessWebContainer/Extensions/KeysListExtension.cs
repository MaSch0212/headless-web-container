using MaSch.Presentation.Wpf.Markup;
using System;
using System.Linq;
using System.Windows.Forms;

namespace HeadlessWebContainer.Extensions
{
    public class KeysListExtension : EnumerationExtension
    {
        public KeysListExtension()
            : base(typeof(Keys))
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return (
                from x in (EnumerationMember[])base.ProvideValue(serviceProvider)
                let key = (Keys)x.Value!
                where key > 0 && (int)key <= 254
                select x).ToArray();
        }
    }
}
