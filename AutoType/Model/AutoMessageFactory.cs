using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AutoType.Model
{
    /// <summary>
    /// This class is in charge of creating the automessages objects, also reading or writing the
    /// xml document that contains the messages data.
    /// </summary>
    class AutoMessageFactory
    {
        private string xmlDocumentPath = "data\\AutoMessages.xml";
        private XmlDocument xmlMessages; //XML object where the data for the messages should be located.

        /// <summary>
        /// Loads the messages from the XML to a dictionary object.
        /// </summary>
        /// <returns>Dictionary[key:string=MessageCode,value:AutoMessage:MessageObject]</returns>
        public Dictionary<string,AutoMessage> loadAutoMessages() 
        {
            if (!File.Exists(xmlDocumentPath))
            {   
                if(!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }
                File.WriteAllText(xmlDocumentPath,"<?xml version=\"1.0\"?><AutoMessages><Message Code=\"firstcode\"></Message></AutoMessages>");
            }
            Dictionary<string, AutoMessage> lMessages = new Dictionary<string, AutoMessage>();
            this.xmlMessages = new XmlDocument();
              
           
            xmlMessages.Load(XmlReader.Create(this.xmlDocumentPath));            
            XmlNodeList messages = xmlMessages.GetElementsByTagName("Message");
            if (messages != null)
            {
                foreach (XmlNode cNode in messages)
                {
                    AutoMessage tempMess = new AutoMessage(cNode.Attributes["Code"].Value, cNode.InnerText);
                    lMessages.Add(tempMess.Code, tempMess);
                }
            }
            return lMessages;
        }

        /// <summary>
        /// Saves a message to the XML using
        /// </summary>
        /// <param name="msg">Message Object to be modified.</param>
        /// <param name="pNewContent">String with new content for the message</param>
        /// <returns>Boolean true if successful, false if an error occurred.</returns>
        public bool saveMessage(AutoMessage msg, string pNewContent)
        {
            try
            {
                this.xmlMessages.SelectSingleNode("//Message[@Code='" + msg.Code + "']").InnerText = pNewContent;
                this.xmlMessages.Save(this.xmlDocumentPath);                
                return true;
            }
            catch (Exception e) 
            {
                //TODO: log error
                return false;
            }
        }

        /// <summary>
        /// Adds a new code
        /// </summary>
        /// <param name="pCode">String code for the new message</param>
        /// <returns>boolean true if success or false if an error occurred.</returns>
        public bool addNewCode(string pCode) 
        {
            try
            {
                XmlDocument tempDoc = new XmlDocument();                                
                XmlElement element = tempDoc.CreateElement("Message");                
                XmlAttribute attr = tempDoc.CreateAttribute("Code");
                attr.Value = pCode;
                element.Attributes.Append(attr);
                element.InnerText = "";                
                XmlNode cNote = this.xmlMessages["AutoMessages"].OwnerDocument.ImportNode(element,true);
                this.xmlMessages["AutoMessages"].AppendChild(cNote);
                this.xmlMessages.Save(this.xmlDocumentPath);
                return true;
            }
            catch (Exception e) 
            {
                //TODO: log error
                return false;
            }
        }

        /// <summary>
        /// Deletes a code and its content from the xml repository
        /// </summary>
        /// <param name="pCode">string: Code of message to be deleted</param>
        /// <returns>boolean: true on success false otherwise</returns>
        public bool deleteCode(string pCode) 
        {
            try 
            {
                this.xmlMessages["AutoMessages"].RemoveChild(this.xmlMessages.SelectSingleNode("//Message[@Code='" + pCode + "']"));
                this.xmlMessages.Save(this.xmlDocumentPath);
                return true;
            }
            catch (Exception e) 
            {
                //TODO: log error
                return false;
            }
        }
    }
}
