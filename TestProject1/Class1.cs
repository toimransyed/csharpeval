using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    public class PropertyExtensibleObject : DynamicObject
    {
        private Dictionary<string, object> addedProperties = new Dictionary<string, object>();

        public Object Target { get; set; }

        public PropertyExtensibleObject(Object target)
        {
            this.Target = target;
            if (target is IList)
            {
                IList temp = target as IList;

                foreach (Object item in temp)
                {
                    PropertyInfo propertyName = item.GetType().GetProperty("Name");
                    string name = propertyName.GetValue(item).ToString();
                    PropertyInfo propertyValue = item.GetType().GetProperty("Value");
                    string value = propertyValue.GetValue(item).ToString();
                    SetValue(name, value);
                }
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            PropertyInfo property = this.Target.GetType().GetProperty(binder.Name);

            if (property != null)
            {
                result = property.GetValue(this.Target);
            }
            else if (addedProperties.ContainsKey(binder.Name))
            {
                result = addedProperties[binder.Name];
            }
            else
            {
                result = null;
                return false;
            }

            return true;

        }

        public void SetValue(string propertyName, Object value)
        {
            PropertyInfo property = this.Target.GetType().GetProperty(propertyName);

            if (property != null)
            {
                property.SetValue(this.Target, value);
            }

            if (addedProperties.ContainsKey(propertyName))
            {
                addedProperties[propertyName] = value;
            }
            else
            {
                addedProperties.Add(propertyName, value);
            }
        }

        //public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        //{

        //}


    }
}
