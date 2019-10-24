
//------------------------------------------------------------------------------
// 
//     This code was generated by a SAP. NET Connector Proxy Generator Version 2.0
//     Created at 2006-12-13
//     Created from Windows
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// 
//------------------------------------------------------------------------------
using System;
//using System.Text;
//using System.Collections;
//using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml.Schema;
//using System.Web.Services;
//using System.Web.Services.Description;
//using System.Web.Services.Protocols;
using SAP.Connector;

namespace RFCProxyBuilder
{

  /// <summary>
  /// RFC: Fields of a table
  /// </summary>
  [RfcStructure(AbapName ="RFC_FIELDS" , Length = 80, Length2 = 140)]
  [Serializable]
  public class RFC_FIELDS : SAPStructure
  {
   

    /// <summary>
    /// Table Name
    /// </summary>
 
    [RfcField(AbapName = "TABNAME", RfcType = RFCTYPE.RFCTYPE_CHAR, Length = 30, Length2 = 60, Offset = 0, Offset2 = 0)]
    [XmlElement("TABNAME", Form=XmlSchemaForm.Unqualified)]
    public string TABNAME
    { 
       get
       {
          return _TABNAME;
       }
       set
       {
          _TABNAME = value;
       }
    }
    private string _TABNAME;


    /// <summary>
    /// Field Name
    /// </summary>
 
    [RfcField(AbapName = "FIELDNAME", RfcType = RFCTYPE.RFCTYPE_CHAR, Length = 30, Length2 = 60, Offset = 30, Offset2 = 60)]
    [XmlElement("FIELDNAME", Form=XmlSchemaForm.Unqualified)]
    public string FIELDNAME
    { 
       get
       {
          return _FIELDNAME;
       }
       set
       {
          _FIELDNAME = value;
       }
    }
    private string _FIELDNAME;


    /// <summary>
    /// Position of field in structure (from 1)
    /// </summary>
 
    [RfcField(AbapName = "POSITION", RfcType = RFCTYPE.RFCTYPE_INT, Length = 4, Length2 = 4, Offset = 60, Offset2 = 120)]
    [XmlElement("POSITION", Form=XmlSchemaForm.Unqualified)]
    public int POSITION
    { 
       get
       {
          return _POSITION;
       }
       set
       {
          _POSITION = value;
       }
    }
    private int _POSITION;


    /// <summary>
    /// Field offset from beginning of structure (from 0)
    /// </summary>
 
    [RfcField(AbapName = "OFFSET", RfcType = RFCTYPE.RFCTYPE_INT, Length = 4, Length2 = 4, Offset = 64, Offset2 = 124)]
    [XmlElement("OFFSET", Form=XmlSchemaForm.Unqualified)]
    public int OFFSET
    { 
       get
       {
          return _OFFSET;
       }
       set
       {
          _OFFSET = value;
       }
    }
    private int _OFFSET;


    /// <summary>
    /// Internal length of field
    /// </summary>
 
    [RfcField(AbapName = "INTLENGTH", RfcType = RFCTYPE.RFCTYPE_INT, Length = 4, Length2 = 4, Offset = 68, Offset2 = 128)]
    [XmlElement("INTLENGTH", Form=XmlSchemaForm.Unqualified)]
    public int INTLENGTH
    { 
       get
       {
          return _INTLENGTH;
       }
       set
       {
          _INTLENGTH = value;
       }
    }
    private int _INTLENGTH;


    /// <summary>
    /// Number of decimal places
    /// </summary>
 
    [RfcField(AbapName = "DECIMALS", RfcType = RFCTYPE.RFCTYPE_INT, Length = 4, Length2 = 4, Offset = 72, Offset2 = 132)]
    [XmlElement("DECIMALS", Form=XmlSchemaForm.Unqualified)]
    public int DECIMALS
    { 
       get
       {
          return _DECIMALS;
       }
       set
       {
          _DECIMALS = value;
       }
    }
    private int _DECIMALS;


    /// <summary>
    /// ABAP Data Type
    /// </summary>
 
    [RfcField(AbapName = "EXID", RfcType = RFCTYPE.RFCTYPE_CHAR, Length = 1, Length2 = 2, Offset = 76, Offset2 = 136)]
    [XmlElement("EXID", Form=XmlSchemaForm.Unqualified)]
    public string EXID
    { 
       get
       {
          return _EXID;
       }
       set
       {
          _EXID = value;
       }
    }
    private string _EXID;

  }

}