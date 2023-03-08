using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class OrderObject
    {

        private string orderNumberField;

        /// <remarks/>
        public string OrderNumber
        {
            get
            {
                return orderNumberField;
            }
            set
            {
                this.orderNumberField = value;
            }
        }

        public OrderObject()
        {
            OrderNumber = "";
        }
    }


}
