using System;
using System.IO;
using System.Collections;

using System.Xml;
using System.Text;

using System.Resources;
using System.Reflection;

namespace RFCProxyBuilder
{
    /// <summary>
    /// RFCProxyGen�� ���� ��� �����Դϴ�.
    /// </summary>
    public class RFCProxyGen : RFCProxyBuild
    {
        #region Member Constants
        private const String RESOURCE_FILENAME = "RFCProxyGen";
        private const String RES_RFC_TABLE_TEMP = "RFC_TABLE_TEMP";
        private const String RES_RFC_STRUCTURE_TEMP = "RFC_STRUCTURE_TEMP";
        private const String RES_RFC_ELEMENTFIELD_TEMP = "RFC_ELEMENTFIELD_TEMP";
        private const String RES_RFC_ARRAYFIELD_TEMP = "RFC_ARRAYFIELD_TEMP";
        private const String RES_CLIENT_PROXY_TEMP = "SAP_CLIENT_PROXY_TEMP";
        private const String RES_SERVER_PROXY_TEMP = "SAP_SERVER_PROXY_TEMP";

        private const String RES_SIMPLE_PARA_TEMP = "SIMPLE_PARA_TEMP";
        private const String RES_TABLE_PARA_TEMP = "TABLE_PARA_TEMP";
        private const String RES_STRUCT_PARA_TEMP = "STRUCT_PARA_TEMP";

        private const String STR_REMOVE_LENGTH = ", Length=[=INTLENGTH=], Length2=[=INTLENGTH2=]";
        private const String STR_REMOVE_DECIMALS = ", Decimals=[=DECIMALS=]";

        private const String XML_ATTRIBUTE_FIELDNAME = "FIELDNAME";
        private const String XML_ATTRIBUTE_EXID = "EXID";
        private const String XML_ATTRIBUTE_INTLENGTH = "INTLENGTH";
        private const String XML_ATTRIBUTE_OFFSET = "OFFSET";
        private const String XML_ATTRIBUTE_TABNAME = "TABNAME";
        private const String XML_ATTRIBUTE_PARAMCLASS = "PARAMCLASS";
        private const String XML_ATTRIBUTE_PARAMETER = "PARAMETER";
        private const String XML_ATTRIBUTE_OPTIONAL = "OPTIONAL";
        private const String XML_ATTRIBUTE_DECIMALS = "DECIMALS";
        private const String XML_ATTRIBUTE_DTYP = "DTYP";
        private const String XML_ATTRIBUTE_ROLLNAME = "ROLLNAME";
        private const String XML_ATTRIBUTE_DBLENGTH = "DBLENGTH";
        private const String XML_ATTRIBUTE_DEPTH = "DEPTH";
        private const String XML_ATTRIBUTE_TABLEN = "Tablen";

        private const String XML_ATTRIBUTE_LENGTHB1 = "LENGTH_B1";
        private const String XML_ATTRIBUTE_LENGTHB2 = "LENGTH_B2";
        private const String XML_ATTRIBUTE_OFFSETB1 = "OFFSET_B1";
        private const String XML_ATTRIBUTE_OFFSETB2 = "OFFSET_B2";

        private const String XML_VALUE_PARAMCLASS_X = "X";
        private const String XML_VALUE_PARAMCLASS_T = "T";
        private const String XML_VALUE_PARAMCLASS_E = "E";

        private const String XPATH_EXCEPTION_PARAM = "//RFC_FUNINTTable[@PARAMCLASS = 'X']";
        private const String XPATH_NOT_EXCEPTION_PARAM = "//RFC_FUNINTTable[@PARAMCLASS != 'X']";
        private const String XPATH_EXIDLPARAMNODE = "*/*[@EXID = 'L']";
        private const String XPATH_EXIDHSUBPARAMNODE = "child::*/child::*[@EXID = 'h' or EXID = 'v' or EXID = 'u']";

        private const String REPLACE_PARAM_INFO = "// Parameters";
        private const String REPLACE_PARAM_NAME = "[=PARAM_NMAE=]";
        private const String REPLACE_PARAM_ORDER = "[=PARAM_ORDER=]"; 

        private const String REPLACE_RESULT = "// [=PARAM_NMAE=]=([=var_type=])results[[=PARAM_ORDER=]];";
        private const String REPLACE_NAMESPACE = "[=NAMESPACE=]";
        private const String REPLACE_CLASSNAME = "[=CLASSNAME=]";
        private const String REPLACE_TYPENAME = "[=TYPENAME=]";
        private const String REPLACE_FIELDNAME = "[=FIELD_NAME=]";
        private const String REPALCE_FIELDXMLNAME = "[=FIELD_XMLNAME=]";
        private const String REPLACE_RFCTYPE = "[=RFC_TYPE=]";
        private const String REPLACE_INTLENGTH = "[=INTLENGTH=]";
        private const String REPLACE_INTLENGTH2 = "[=INTLENGTH2=]";
        private const String REPLACE_OFFSET = "[=OFFSET=]";
        private const String REPLACE_OFFSET2 = "[=OFFSET2=]";
        private const String REPLACE_DECIMALS = "[=DECIMALS=]";
        private const String REPLACE_VARTYPE = "[=var_type=]";
        private const String REPLACE_ABAPNAME = "[=ABAP_NAME=]";
        private const String REPLACE_XMLPARAMNAME = "[=XML_PARAM_NMAE=]";
        private const String REPLACE_PROXYFUNCNAME = "[=PROXY_FUNC_NAME=]";
        private const String REPLACE_PROXYXMLNAME = "[=PROXY_XML_NAME=]";
        private const String REPLACE_DIRECTION = "[=DIRECTION=]";
        private const String REPLACE_OPTIONAL = "[=OPTIONAL=]";

        private const String REPLACE_EXCEPT_INFO = "// public const string [=EXCEPTION=]=\"[=EXCEPTION=]\";";
        private const String REPLACE_EXCEPT = "[=EXCEPTION=]";

        //private const String XPATH_DISTINCT_TABNAME = "//RFC_FUNINTTable[@TABNAME !='' and not(@TABNAME=preceding-sibling::RFC_FUNINTTable/@TABNAME)]/@TABNAME";
        private const String XPATH_TABLESTRUCT_NODE = "//*[@TABNAME !='' and ((@EXID = '' and @PARAMCLASS != 'X') or @EXID = 'u' or @EXID = 'h' or @EXID = 'v')]";
        private const String XPATH_X031LROOT_NODE = "//ROOT_X031LTable";
        private const String XPATH_X030LTABLE_NODE = ".//X030LTable";
        #endregion

        #region Member VARIABLE
        private ArrayList m_arrSAPStructClass = null;
        private ArrayList m_arrSAPTableClass = null;
        private String m_strFuncProxyClass = null;
        private ArrayList m_arrProcessedTableName = null;
        private ArrayList m_arrProcessedStructName = null;
        private ResourceManager m_oRM = null;
        #endregion

        public RFCProxyGen()
        {
            m_arrSAPStructClass = new ArrayList();
            m_arrSAPTableClass = new ArrayList();
            m_arrProcessedTableName = new ArrayList();
            m_arrProcessedStructName = new ArrayList();

            OpenResource();
        }

        ~RFCProxyGen()
        {
            if (m_arrSAPStructClass != null)
                m_arrSAPStructClass.Clear();

            if (m_arrSAPTableClass != null)
                m_arrSAPTableClass.Clear();

            if (m_arrProcessedTableName != null)
                m_arrProcessedTableName.Clear();

            if (m_arrProcessedStructName != null)
                m_arrProcessedStructName.Clear();

            CloseResource();
        }

        /// <summary>
        /// ���ҽ� �Ŵ����� �����Ͽ�, ���ҽ��� �����Ѵ�.
        /// </summary>
        private void OpenResource()
        {
            String strResourceName = null;
            // Resource �̸��� ����. �ַ�ǿ� "RFCProxyGen.resource" ������ �ݵ�� �־�� �Ѵ�.
            strResourceName = String.Format("{0}.{1}", this.GetType().Namespace, RESOURCE_FILENAME);

            // ���ҽ��� �����ϱ� ���Ͽ� �����ڸ� �����Ѵ�.
            if (m_oRM == null)
                m_oRM = new ResourceManager(strResourceName, this.GetType().Assembly);
        }

        /// <summary>
        /// ����� ���ҽ��� �����Ѵ�.
        /// </summary>
        private void CloseResource()
        {
            if (m_oRM != null)
            {
                m_oRM.ReleaseAllResources();
                m_oRM = null;
            }
        }

        #region ���� ��ȯ �Լ�

        /// <summary>
        /// EXID ���� ����ü �ۼ� �ܰ迡�� ����� SAP RFC ������ ����, C# ������ ���� ���ڿ��� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="strEXIDVal">EXID ��</param>
        /// <param name="strRFCType">RFC ������ ���� ���ڿ�</param>
        /// <param name="strCSType">C# ������ ���� ���ڿ�</param>
        private void ConvEXID2Type(string strEXIDVal, ref String strRFCType, ref String strCSType)
        {
            switch (strEXIDVal)
            {
                case "C":
                    strRFCType = "RFCTYPE_CHAR";
                    strCSType = "String";
                    break;
                case "I":
                    strRFCType = "RFCTYPE_INT";
                    strCSType = "Int32";
                    break;
                case "F":
                    strRFCType = "RFCTYPE_FLOAT";
                    strCSType = "Double";
                    break;
                case "D":
                    strRFCType = "RFCTYPE_DATE";
                    strCSType = "String";
                    break;
                case "T":
                    strRFCType = "RFCTYPE_TIME";
                    strCSType = "String";
                    break;
                case "P":
                    strRFCType = "RFCTYPE_BCD";
                    strCSType = "Decimal";
                    break;
                case "N":
                    strRFCType = "RFCTYPE_NUM";
                    strCSType = "String";
                    break;
                case "X":
                    strRFCType = "RFCTYPE_BYTE";
                    strCSType = "Byte[]";
                    break;
                case "b":
                    strRFCType = "RFCTYPE_INT1";
                    strCSType = "Byte";
                    break;
                case "s":
                    strRFCType = "RFCTYPE_INT2";
                    strCSType = "Int16";
                    break;
                case "g":
                    strRFCType = "RFCTYPE_STRING";
                    strCSType = "String";
                    break;
                case "y":
                    strRFCType = "RFCTYPE_XSTRING";
                    strCSType = "Byte[]";
                    break;
                case "v":
                case "h":
                    strRFCType = "RFCTYPE_XMLDATA";
                    break;
                case "u":
                    strRFCType = "RFCTYPE_STRUCTURE";
                    break;
            } // switch
            return;
        }

        #endregion ���� ��ȯ �Լ�

        // A typed collection of RFC_FIELDS elements.
        /// <summary>
        /// ������ ������ ���̺��� ������ �� �ֵ��� �÷��� Ŭ������ �����.
        /// </summary>
        /// <param name="strNamespace">���ӽ����̽� �̸�</param>
        /// <param name="strTableName">���̺� ���� ���ڿ�</param>
        /// <param name="nInitialSize">�迭 ������ �ʱ� ������</param>
        /// <param name="strTypeName">���ڵ� ������ ���� ���ڿ�</param>
        /// <param name="bOutSource">�ҽ� ��� ����</param>
        /// <param name="strOutputPath">��� ���͸�</param>
        private void GenRFCTableCollectionClass(String strNamespace, String strTableName, String strTypeName, 
                                                int nInitialSize, bool bOutSource, String strOutputPath)
        {
            StreamWriter oSW = null;     // ��Ʈ�� �ۼ�
            String strFilePath = null;   // ���� �̸� �� ���
            String strClassSource = null;// ������ Ŭ���� �ҽ�

            // Ŭ���� �ҽ��� �����Ѵ�.
            try
            {
                // �̸��� "/" ���� ����� �� �����Ƿ� "_"���� ��ü�Ѵ�.
                strFilePath = (bOutSource) ? strOutputPath + strTableName.Replace("/", "_") + ".CS" : null;

                // ======================= HARD-CODING SECTION =======================
                // ���ø� �ڵ��� String ���� ��ȯ�մϴ�.   
                // "RFCProxyGen.resource" ���Ͽ� "RFC_TABLE_TEMP" �׸��� �ݵ�� �־�� �Ѵ�.
                strClassSource = m_oRM.GetString(RES_RFC_TABLE_TEMP);

                // ���ӽ����̽��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_NAMESPACE, strNamespace);
                // ���̺��� ���� �̸��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_CLASSNAME, strTableName.Replace("/", "_"));
                // ���ڵ� ���� �̸��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_TYPENAME, strTypeName.Replace("/", "_"));
                // "return new String();" ���ڿ��� ��ü�Ѵ�.
                strClassSource = strClassSource.Replace("new String();", "\"\";");
                // "return new Byte[]();" ���ڿ��� ��ü�Ѵ�.
                strClassSource = strClassSource.Replace("return new Byte[]();", 
                                                String.Format("return new Byte[{0}];", nInitialSize));
                // ======================= HARD-CODING SECTION =======================

                if (!String.IsNullOrEmpty(strFilePath)) // ���� �̸��� �����Ǹ�
                {
                    // ���� �̸��� ��� ����
                    oSW = File.CreateText(strFilePath);
                    oSW.Write(strClassSource);

                }
                
                // ����� ������ Ŭ������ �߰��Ѵ�.
                m_arrSAPTableClass.Add(strClassSource);
            }
            catch (IOException iexp)
            {
                Console.Write(iexp.Message);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (oSW != null) oSW.Close();
            }
            return;
        }

        /// <summary>
        /// RFC Table ���� Ŭ���� ���ø� �ҽ� �ʵ� ������ �߰��ϰ�, �ι�° ���̸� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="oFieldNodeList">�ʵ� ����� ����Ʈ</param>
        /// <param name="strClassSource">Ŭ���� ���ڿ�</param>
        /// <returns>����ü�� �ι�° ����, Length2</returns>
        private int AddFieldInfo2StructClass(XmlNodeList oFieldNodeList, ref String strClassSource)
        {
            XmlNode oFieldNode = null;      // �ʵ� ���
            String strElemFieldTemp = null; // XmlElement ������ �ʵ�
            String strArrayFieldTemp = null;// XmlArray ������ �ʵ�
            String strFieldSource = null;   // �ʵ��� �ҽ�

            String strEXIDVal = null;  // �ʵ� ����� EXID ��
            String strAbapName = null; // ABAP Field Name
            String strRFCType = null;  // RFC ����
            String strCSType = null;   // CS ����

            String strDecimals = null; // �ʵ��� DECIMALS

            String strFieldDepth = null; // ù��° �ʵ��� DEPTH ��
            String strDepth = null;   // �ʵ��� DEPTH ��
            String strLength = null; int nLength = 0;   // �ʵ��� ����
            String strOffset = null; int nOffset = 0;   // �ʵ��� Offset
            String strLength2 = null;int nLength2 = 0;  // �ʵ��� ����
            String strOffset2 = null;int nOffset2 = 0;  // �ʵ��� Offset

            int nStructLength2 = 0;   // ����ü�� ũ��

            try
            {
                // ���ҽ����� �ʵ� ���ø��� �����´�.
                strElemFieldTemp = m_oRM.GetString(RES_RFC_ELEMENTFIELD_TEMP);
                strArrayFieldTemp = m_oRM.GetString(RES_RFC_ARRAYFIELD_TEMP);

                // �ʵ��� ó�� ��忡�� DEPTH ���� �����´�.
                if (oFieldNodeList.Count >= 1 )
                    strFieldDepth = oFieldNodeList[0].Attributes[XML_ATTRIBUTE_DEPTH].Value;

                // �ʵ� ��带 �ݺ��Ͽ� ó���Ѵ�.
                for (int i = 0; i < oFieldNodeList.Count; ++i)
                {
                    oFieldNode = oFieldNodeList[i];

                    // �ʵ� ����� EXID ���� "L"�̸�, ó������ �ʴ´�.
                    strEXIDVal = oFieldNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                    // EXID ���� "L"�̰ų� ""�̸� ó������ �ʴ´�.
                    if (strEXIDVal.Equals("L") || strEXIDVal.Equals("")) continue;

                    // �ʵ� ����� ABAP �̸��� �����´�.
                    strAbapName = oFieldNode.Attributes[XML_ATTRIBUTE_FIELDNAME].Value;
                    // �ʵ� ����� DECIMALS ���� �����´�.
                    strDecimals = oFieldNode.Attributes[XML_ATTRIBUTE_DECIMALS].Value;
                    // �ʵ� ����� DEPTH ���� �����´�.
                    strDepth = oFieldNode.Attributes[XML_ATTRIBUTE_DEPTH].Value;
                   
                    // ����ü�� ���̿� OFFSET ��ġ�� ���Ѵ�.
                    strLength = oFieldNode.Attributes[XML_ATTRIBUTE_DBLENGTH].Value;
                    strOffset = oFieldNode.Attributes[XML_ATTRIBUTE_OFFSET].Value;

                    // ���� ����� LENGTH2 ���� OFFSET2 ���̸� ���Ͽ� ���� ����� OFFSET2 ���� ���Ѵ�.
                    if (String.Equals(strFieldDepth, strDepth))
                        nOffset2 += nLength2;

                    // ���� ����� ���� ���������� ��ȯ�Ѵ�.
                    nLength = Convert.ToInt32(strLength);
                    nOffset = Convert.ToInt32(strOffset);
                    
                    // ���� ����� ���̸� ���Ѵ�.
                    // RFC(ABAP) ���� �߿��� .NET String ���·� ��ȯ�Ǵ� ����
                    // [C (String)], [D (Date)], [T (Time)], [N (Numc)], [RFC String], [XString]
                    switch (strEXIDVal)
                    {
                        case "h":
                        case "g":
                        case "u": // Length2 ���� Length ���� �����Ѵ�.
                            nLength2 = nLength;
                            break;
                        case "v":
                            // ���߿� ���� �ʿ���.
                            break;
                        case "C":
                        case "D":
                        case "T":
                        case "N":
                            nLength2 = nLength * 2;
                            break;
                        default:
                            nLength2 = nLength;
                            break;
                    }
                    strLength2 = nLength2.ToString();
                    // STRUCT_LENGTH ���̸�  ����Ѵ�.
                    nStructLength2 += nLength2; // Offset ����, ���� ��ġ�� ����Ѵ�.
   
                    // ù��° ����� DEPTH ���� ���� ����� DEPTH ���� ���Ͽ�, ���� �ٸ��� ó������ �ʴ´�.
                    if (String.Equals(strFieldDepth, strDepth))
                    {
                        // ���� ���� ����� OFFSET ���� OFFSET2 ������ ū ���, OFFSET2 ���� �����Ѵ�.
                        if (nOffset > nOffset2) nOffset2 = nOffset;
                        // STRUCT_LENGTH2 ���� ���� ���� ���Ѵ�.
                        nStructLength2 += (nOffset2 % 2);
                        // OFFSET2 ���� ¦���� �����.
                        nOffset2 += (nOffset2 % 2);
                        strOffset2 = nOffset2.ToString();
                    }
                    else
                    {
                        continue;
                    }

                    // �ʵ� ����� EXID ���� "h" �Ǵ� "v" (RFCTYPE_XMLDATA ����)�̸�,
                    // strArrayFieldTemp ������ ����Ѵ� ������ ������ strElemFieldTemp
                    strRFCType = strCSType = null; // ���� ���� �ʱ�ȭ
                    ConvEXID2Type(strEXIDVal, ref strRFCType, ref strCSType);
                    switch (strEXIDVal)
                    {
                        case "h":
                            strFieldSource = strArrayFieldTemp;
                            strCSType = oFieldNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                            break;
                        case "v":
                        case "u":
                            strFieldSource = strElemFieldTemp;
                            strCSType = oFieldNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                            break;

                        default:
                            strFieldSource = strElemFieldTemp;
                            break;
                    }

                    // �ʵ� �ҽ��� ���ڿ��� ��ü�Ѵ�.
                    ReplaceFieldSource(ref strFieldSource, strAbapName,
                                        strEXIDVal, strRFCType, strCSType, strDecimals,
                                        strLength, strLength2, strOffset, strOffset2);
                    // Ŭ���� �ҽ��� ���ڿ��� ��ü�Ѵ�.
                    strClassSource = strClassSource.Replace(REPLACE_PARAM_INFO, strFieldSource + "\n" + REPLACE_PARAM_INFO);
                   
               } // for
            }
            catch (Exception exp)
            {
                throw exp;
            }
            // ����ü ũ�⸦ ��ȯ�Ѵ�.
            return nStructLength2;
        }

        /// <summary>
        /// ���ڿ��� ��ä�Ѵ�.
        /// </summary>
        private void ReplaceFieldSource(ref String strFieldSource, String strAbapName,
                                        String strEXIDVal, String strRFCType, String strCSType, String strDecimals,
                                        String strLength, String strLength2, String strOffset, String strOffset2)
        {
            String strXMLName = null;
            String strFieldName = null;
            String strCShapType = null;

            try
            {
                // EXID ���� "u"�̸� �ʵ� �ҽ����� ���̸� �����Ѵ�.
                if (strEXIDVal.Equals("u"))
                    strFieldSource = strFieldSource.Replace(STR_REMOVE_LENGTH, "");
                // DECIMALS ���� 0 �̸� �����Ѵ�.
                if (strDecimals.Equals("0"))
                    strFieldSource = strFieldSource.Replace(STR_REMOVE_DECIMALS, "");

                strCShapType = (strCSType.Equals("")) ? strAbapName : strCSType;

                strCShapType = strCShapType.Replace("/", "_");
                strCShapType = strCShapType.Replace("%", "_");
                strCShapType = strCShapType.Replace("$", "_");

                // "/" ���ڸ� "_" ���ڷ�, "%" ���ڸ� "_--25" ���ڷ� ��ü�Ѵ�.
                strXMLName = strAbapName.Replace("/", "_");
                strXMLName = strXMLName.Replace("#", "_--23");
                strXMLName = strXMLName.Replace("$", "_--24");
                strXMLName = strXMLName.Replace("%", "_--25");
                strXMLName = strXMLName.Replace("&", "_--26");
                // strXMLName ���� ���ڷ� �����ϸ�, "_--3" ���ڸ� �߰��Ѵ�.
                if (Char.IsNumber(strXMLName, 0)) strXMLName = "_--3" + strXMLName;

                // "/" ���ڸ� "_" ���ڷ�,"%" ���ڸ� "__" ���ڷ� ��ü�Ѵ�.
                // CS �������� �浹�� ���ϱ� ���� �տ� "_" �� ���δ�.
                strFieldName = strAbapName.Replace("#", "__");
                strFieldName = strFieldName.Replace("/", "_").ToLower();
                strFieldName = strFieldName.Replace("$", "_").ToLower();
                strFieldName = strFieldName.Replace("%", "__").ToLower();
                strFieldName = strFieldName.Replace("&", "_").ToLower();
                strFieldName = Format1stUpper(strFieldName);
                // strFieldName ���� ���ڷ� �����ϸ�, "N" ���ڸ� �߰��Ѵ�.
                if (Char.IsNumber(strFieldName, 0)) strFieldName = "N" + strFieldName;
                //strFieldName = "_" + strFieldName[0].ToString().ToUpper() + strFieldName.Substring(1);

                // �ʵ� �ҽ��� �����Ѵ�.
                strFieldSource = strFieldSource.Replace(REPLACE_INTLENGTH2, strLength2);   // �ι�° �ʵ� ���̸� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_OFFSET2, strOffset2);      // �ι�° OFFSET ��ġ�� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_ABAPNAME, strAbapName);    // ABAP �̸��� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPALCE_FIELDXMLNAME, strXMLName); // XML ��� �̸��� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_FIELDNAME, strFieldName);  // �ʵ� �̸��� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_RFCTYPE, strRFCType);      // RFC ���� �̸��� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_INTLENGTH, strLength);     // �ʵ� ������ �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_DECIMALS, strDecimals);    // DECIMALS ���� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_OFFSET, strOffset);        // offset ��ġ�� �ٲ۴�.
                strFieldSource = strFieldSource.Replace(REPLACE_VARTYPE, strCShapType);    // C# ���� ���� �̸��� �ٲ۴�.
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// Field �̸��� �����Ѵ�.
        /// </summary>
        /// <param name="strFieldName">���� �ʵ� �̸�</param>
        /// <returns>������ �ʵ� �̸�</returns>
        private String Format1stUpper(String strFieldName)
        {
            int nPos = 0; // "_" ������ ��ġ
            String strRetVal = null; // ������ �ʵ��� �̸�
            Char[] chFieldName = null;

            chFieldName = strFieldName.ToCharArray();

            nPos = -1;
            do
            {
                // "_" ������ ���� ��ġ�� ���ڸ� �빮�ڷ� ��ģ��.
                if (chFieldName[nPos + 1] >= 'a')
                {
                    chFieldName[nPos + 1] = (Char)(chFieldName[nPos + 1] - 0x20);
                }

                nPos = strFieldName.IndexOf("_", nPos+1);
            } while (nPos >= 0 && nPos < (chFieldName.Length -1));

            strRetVal = new String(chFieldName);

            return strRetVal;
        }

        /// <summary>
        /// RFC ����(Struct) Ŭ������ �����. ���ҽ�(RFCProxyGen.resources)�� ����Ѵ�.
        /// </summary>
        /// <param name="oFieldNodeList">���� ������ ������ �ִ� XML ������ ��� ����Ʈ</param>
        /// <param name="strNamespace">���ӽ����̽� �̸�</param>
        /// <param name="strAbapName">����ü Ŭ������ ABAP �̸�</param>
        /// <param name="strLength">����ü�� ����</param>
        /// <param name="bOutSource">�ҽ� ��� ����</param>
        /// <param name="strOutPath">��� ���͸�</param>
        /// <returns>������ Ŭ���� ���ڿ�</returns>
        private void GenRFCStructClass(XmlNodeList oFieldNodeList, String strNamespace, String strAbapName,
                                       String strLength, bool bOutSource, string strOutPath)
        {
            String strClassSource = null;// RFC ���� ���� Ŭ���� ���ڿ�

            String strClassName = null;  // SAPStruct Ŭ���� �̸�
            String strFilePath = null;   // ��� ������ ��ο� �̸�
            StreamWriter oSW = null;     // ��Ʈ�� �ۼ�

            int nLength2 = 0;            // ����ü�� Length2 ��

            try
            {
                // "RFCProxyGen.resource" ���Ͽ��� ����ü �ҽ� ���ø��� �ε��Ѵ�.
                strClassSource = m_oRM.GetString(RES_RFC_STRUCTURE_TEMP);

                // Ŭ���� �̸��� "/" ���ڸ� ����� �� �����Ƿ� "_" ���ڷ� ��ü�Ѵ�.
                strClassName = strAbapName.Replace("/", "_");

                // �ҽ� ���ø��� ���ڿ��� ��ü�Ѵ�.
                strClassSource = strClassSource.Replace(REPLACE_NAMESPACE, strNamespace);  // ���� �����̽��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_ABAPNAME,  strAbapName);   // ABAP ���̺� �̸��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_CLASSNAME, strClassName);  // Ŭ���� �̸��� �ٲ۴�.
                strClassSource = strClassSource.Replace(REPLACE_INTLENGTH, strLength);     // ���̺� ������ �ٲ۴�.

                // ���ø� �ڵ忡 �ʵ� ������ �߰��Ѵ�. �Լ��� ��ȯ ���� ����ü�� Length2 ���̴�.
                nLength2 = AddFieldInfo2StructClass(oFieldNodeList, ref strClassSource);
                if (Convert.ToInt32(strLength) > nLength2) nLength2 = Convert.ToInt32(strLength);
                strClassSource = strClassSource.Replace(REPLACE_INTLENGTH2, nLength2.ToString());// ���̺� ������ �ٲ۴�.

                // ����� ������ Ŭ������ �߰��Ѵ�.
                m_arrSAPStructClass.Add(strClassSource);

                if (bOutSource)
                {
                    // ���� �̸��� ��� ����
                    strFilePath = strOutPath + strAbapName.Replace("/", "_") + ".CS";
                    oSW = File.CreateText(strFilePath);
                    oSW.Write(strClassSource);
                }
            }
            catch (IOException iexp)
            {
                Console.Write(iexp.Message);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (oSW != null) oSW.Close();
            }
            return;
        }

        /// <summary>
        /// RFC �Լ��� Proxy Ŭ������ �����Ѵ�.
        /// </summary>
        /// <param name="oXmlElem">���� ������ ������ �ִ� XML ���� ���</param>
        /// <param name="strNamespace">���ӽ����̽� �̸�</param>
        /// <param name="strClassName">Ŭ���� �̸�</param>
        /// <param name="strAbapName">ABAP �̸�</param>
        /// <param name="bClientProxy">Proxy ����, true:Client, false:Server</param>
        /// <param name="bOutSource">�ҽ� ���� ��� ����</param>
        /// <param name="strOutPath">�ҽ� ���� ��� ���</param>
        /// <returns>������ Ŭ���� ���ڿ�</returns>
        private void GenRFCFunctionProxy(XmlElement oXmlElem, String strNamespace, String strClassName, String strAbapName,
                                         bool bClientProxy, bool bOutSource, String strOutPath)
        {
            String strProxyClass = null;  // ���ҽ����� ȹ���� Proxy Ŭ���� Template
            XmlNodeList oNodeList = null; // ���̺� ������ ��� ����Ʈ
            String strXmlPath = null;     // XML ���� �˻��� ���� XPATH ���ڿ�

            String strProxyFuncName = null; // Proxy ȣ�� �Լ� �̸�
            String strProxyXMLName = null;  // Proxy XML Meta ������ ���� �Լ� �̸�
            StreamWriter oSW = null;        // ��Ʈ�� �ۼ�
           
            String strProxyFileFullName = null;    // Proxy ������ ��ο� �̸�

            try
            {
                // ����, Ŭ���̾�Ʈ ���Ŀ� �´� �ҽ� ���ø��� �����´�.
                strProxyClass = bClientProxy ? 
                                m_oRM.GetString(RES_CLIENT_PROXY_TEMP) : m_oRM.GetString(RES_SERVER_PROXY_TEMP);
                strProxyClass = strProxyClass.Replace(REPLACE_NAMESPACE, strNamespace); // ���� �����̽��� �ٲ۴�.
                strProxyClass = strProxyClass.Replace(REPLACE_CLASSNAME, strClassName); // Ŭ���� �̸��� �ٲ۴�.
                strProxyClass = strProxyClass.Replace(REPLACE_ABAPNAME,  strAbapName);  // RFC �Լ�(ABAP) �̸��� �ٲ۴�.

                // �Լ� �̸��� �������� �����ϸ� �տ� "_" ���ڸ� ���δ�.
                strProxyFuncName = (strAbapName[0] >= 0x30 && strAbapName[0] <= 0x39) ? "_" + strAbapName : strAbapName;
                // XML ����� ���Ǵ� �̸��� ��ü�Ѵ�.
                strProxyXMLName = strProxyFuncName.Replace("/", "_-");
                // �Լ� �̸��� "/" ���� ������ "_-" ���ڷ� ��ü�Ѵ�.
                strProxyFuncName = strProxyFuncName.Replace("/", "_");
                // RFC Proxy �Լ� �̸��� XML �̸��� �ٲ۴�.
                strProxyClass = strProxyClass.Replace(REPLACE_PROXYXMLNAME, strProxyXMLName);
                strProxyClass = strProxyClass.Replace(REPLACE_PROXYFUNCNAME, strProxyFuncName);

                if (oXmlElem != null)
                {
                    // XML ��忡�� Exception ���� ��带 �˻��Ͽ�, Ŭ���� ������ �߰��Ѵ�.
                    strXmlPath = XPATH_EXCEPTION_PARAM;
                    oNodeList = oXmlElem.SelectNodes(strXmlPath); // ��� ����Ʈ�� �����´�.
                    if (oNodeList.Count > 0)
                        AddExceptParam2ProxyClass(oNodeList, ref strProxyClass);

                    // XML ��忡�� �Ϲ� ����(Exception ����)�� ��带 �˻��Ͽ�, Ŭ���� ������ �߰��Ѵ�.
                    strXmlPath = XPATH_NOT_EXCEPTION_PARAM;
                    oNodeList = oXmlElem.SelectNodes(strXmlPath); // ��� ����Ʈ�� �����´�.
                    if (oNodeList.Count > 0)
                        AddNormalParam2ProxyClass(oNodeList, bClientProxy, ref strProxyClass);
                }

                // ��� �ҽ����� Temp Line ����
                strProxyClass = strProxyClass.Replace(REPLACE_EXCEPT_INFO, "");
                strProxyClass = strProxyClass.Replace(REPLACE_RESULT, "");
                strProxyClass = strProxyClass.Replace(REPLACE_PARAM_INFO, "");
                strProxyClass = strProxyClass.Replace(REPLACE_PARAM_NAME, "");
                
                // ��� ������ ������ Ŭ���� �ҽ��� �����Ѵ�.
                m_strFuncProxyClass = strProxyClass;
                
                if (bOutSource) // ���� �̸��� �����Ǹ�
                {
                    // RFC �Լ��� Proxy Ŭ������ �����Ѵ�.
                    strProxyFileFullName = strOutPath + strProxyFuncName + "Proxy.CS";

                    // ���� �̸��� ��� ����
                    oSW = File.CreateText(strProxyFileFullName);
                    oSW.Write(strProxyClass);
                }
            }
            catch (IOException iexp)
            {
                Console.Write(iexp.Message);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (oSW != null) oSW.Close();
            }
            return;
        }

        /// <summary>
        /// Proxy Ŭ���� ���ø� �ڵ忡 �Ϲ� ���� ������ �߰��Ͽ� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="strSRC">Proxy Ŭ���� ���ø� �ڵ�</param>
        /// <param name="bClientProxy">Proxy ����, true:Client, false:Server</param>
        /// <param name="strClassSource">Ŭ���� �ҽ� �ڵ�</param>
        /// <returns>�Ϲ� ���� ������ �߰��� Ŭ���� ���ڿ�</returns>
        private void AddNormalParam2ProxyClass(XmlNodeList oNodeList, bool bClientProxy, ref String strClassSource)
        {
            String strSimpleParamTemp = null; // �⺻, XmlElement ������ ���� ����
            String strTableParamTemp = null;  // XmlArray ������ ���� ����
            String strStructParamTemp = null; // ����ü XmlElement ������ ���� ����
            
            String strParamInfo = null;  // ������ �Լ��� �Ķ���� ����� ����
            String strFuncParams = null; // �Լ��� ������ ����

            #region ���ø����� ��ü�� ���ڿ��� ���� ����
            String strAbapName = null;    // ABAP �̸�, PARAMETER ����� ��
            String strRFCType = null;     // RFC ����, EXID ���� �̿��Ͽ� ���Ѵ�.
            String strCSTypeName = null;  // C# ���� ���� �̸�, EXID ������ �⺻ ����, XMLDATA, ITAB ������ TABNAME ��.
            String strOptional = null;    // OPTIONAL
            String strLength = null;      // INTLENGTH
            String strParamName = null;   // �ĸ����� �̸�, ABAP �̸����� ��ȯ�� ���� �̸�
            String strXMLParamName = null;// XML ��ҿ��� ���� �Ķ���� �̸�
            String strParamClass = null;  // PARAMCLASS
            String strEXID = null;        // EXID
            String strResult = null; // Proxy �Լ��� ��ȯ�ؾ� �� ��� ���ڿ� ����
            #endregion

            int nOutOrder = -1;   // ������ "OUT"���� ������ ������ ����
            
            XmlNode oFieldNode = null;      // �Ķ���� ��� RFC_FUNINTTable
            XmlNode oX031TableNode = null;  // ���� �ʵ��� ���
          
            String strX031TabName = null;   // X031Table ����� TABNAME ��
            String strX031RollName = null;  // X031Table ����� ROLLNAME ��
            bool bSameTabName = false;      // RFC_FUNINTTable ���� X031Table ����� TABNAME ���� ��ġ ����
            int nX031NodeCnt = 0;           // X031Table ����� �̸�

            int nParamKind = 0;    // ������ ����, 0: Simple, 1: Table, 2: Structure
            ArrayList arProcessedParamName= null; // ó���� �Ķ���� �̸��� ����
            bool bProcessed = false;

            String strToken = ",\n"; // ��ȯ�� ��� ���տ��� ����� ��ū ���ڿ�

            try
            {
                // ���ҽ����� ���ø� �ڵ带 �����´�.
                strSimpleParamTemp = m_oRM.GetString(RES_SIMPLE_PARA_TEMP);
                strTableParamTemp = m_oRM.GetString(RES_TABLE_PARA_TEMP);
                strStructParamTemp = m_oRM.GetString(RES_STRUCT_PARA_TEMP);

                // PARAMETER �̸��� ���� ��带 ã�´�. ���� �� ������ PARAMCLASS ���� ���� "I", "E" �̸�
                // ref �������� �����ϰ�, ��� �Ķ���Ϳ� �߰��Ѵ�.

                arProcessedParamName = new ArrayList(oNodeList.Count);
                for (int i = 0; i < oNodeList.Count; ++i)
                {
                    strX031RollName = strX031TabName = null;
                    bSameTabName = false;
                    nX031NodeCnt = 0;

                    oFieldNode = oNodeList[i]; // ��带 �����´�.
                    // ��忡�� ���� �����´�.
                    strParamClass = oFieldNode.Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                    strAbapName = oFieldNode.Attributes[XML_ATTRIBUTE_PARAMETER].Value;
                    strCSTypeName = oFieldNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                    strEXID = oFieldNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                    strLength = oFieldNode.Attributes[XML_ATTRIBUTE_INTLENGTH].Value;
                    strOptional = oFieldNode.Attributes[XML_ATTRIBUTE_OPTIONAL].Value;
                    // X031LTable ��忡�� TABNAME ���� �����´�.
                    if (oFieldNode.HasChildNodes && oFieldNode.SelectSingleNode("." + XPATH_X031LROOT_NODE).HasChildNodes)
                    {
                        oX031TableNode = oFieldNode.SelectSingleNode("." + XPATH_X031LROOT_NODE).FirstChild;
                        strX031TabName = oX031TableNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                        strX031RollName = oX031TableNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;

                        bSameTabName = String.Equals(strCSTypeName, strX031TabName);
                        nX031NodeCnt = oFieldNode.FirstChild.ChildNodes.Count;
                    }

                    strXMLParamName = strParamName = strAbapName.Replace("/", "_"); // "/" ���ڸ� "_" ���ڷ� ��ü
                    if (Char.IsNumber(strParamName, 0)) // ù ���ڰ� ���ڷ� �����ϸ�,
                    {
                        // �Ķ���� �̸��� "N" ���ڸ� �߰��Ѵ�.
                        strParamName = "N" + strParamName;
                        // XML �Ķ���� �̸��� "_--3"�� �߰��Ѵ�.
                        strXMLParamName = "_--3" + strXMLParamName;
                    }
                    // ������ ������ �̸��� ���Ѵ�.
                    ConvEXID2Type(strEXID, ref strRFCType, ref strCSTypeName);

                    #region �ߺ� ó�� ����

                    // �Ķ���� �̸��� ���� ������ �˻��Ѵ�. ���� XML ������ ���� �̸����� ǥ�õǴ� ������ �ִ�.
                    for (int j = i + 1; j < oNodeList.Count; ++j)
                    {
                        if (String.Equals(strAbapName,oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMETER].Value))
                        {
                            // strParamClass ���� �ٸ��� ���δ�. ������� �ִ� �α��ڷ� "EI" �Ǵ� "IE" ������ ����ȴ�.
                            if (strParamClass.IndexOf(oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMCLASS].Value) < 0)
                                strParamClass += oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                        }
                    }
                    // �̹� ó���� �̸��� �Ķ���Ͱ� ������ ó������ �ʴ´�.
                    bProcessed = false;
                    for (int j = 0; j < arProcessedParamName.Count; ++j)
                    {
                        if (!String.Equals(strParamName, (string)arProcessedParamName[j]))
                            continue;
                        
                        bProcessed = true;
                        break;
                    }
                    // ó������ ���� ���, �̸��� �迭�� �߰��Ѵ�. �ߺ� ����.
                    if (!bProcessed)
                        arProcessedParamName.Add((Object)strParamName);
                    else
                        continue;
                    
                    #endregion �ߺ� ó�� ����

                    nParamKind = 0;
                    
                    // PARAMCLASS = "T" �����̰ų�, EXID ���� �Ʒ��� ���� ���, Table ���°� �ȴ�.
                    if (strParamClass.Equals(XML_VALUE_PARAMCLASS_T) || strEXID.Equals("h"))
                        nParamKind = 1; // Table
                    else if (strEXID.Equals("v") || strEXID.Equals("u"))
                        nParamKind = 2; // Structure

                    strToken = (i == 0) ? "\n" : ",\n";

                    switch (nParamKind)
                    {
                        case 0:
                            strParamInfo = strToken + strSimpleParamTemp;
                            break;
                        case 1:
                            // 1. ���� ����� TABNAME ���� ���� ����� TABNAME ���� ����    
                            // 2. ���� ����� ������ 1 ���� ũ��
                            // 3. ROLLNAME ���� ""�� �ƴϸ�...
                            if (bSameTabName && (nX031NodeCnt >= 1) && !String.Equals(strX031RollName, "")) {
                                strCSTypeName += "Table";
                                strRFCType = "RFCTYPE_ITAB";
                            }
                            strParamInfo = strToken + strTableParamTemp;
                            break;
                        case 2:
                            strParamInfo = strToken + strStructParamTemp;
                            break;
                    }
                    strParamInfo += REPLACE_PARAM_INFO;

                    // ������ ���� ���ڿ��� �����Ѵ�.
                    strCSTypeName = strCSTypeName.Replace("/", "_");// ���� �̸��� "/" ���� ���� �� �� �����Ƿ� "_" ������ ��ü�Ѵ�.
                    
                    // �Ķ���� ������ �����Ѵ�.
                    strParamInfo = this.GenParameterInfo(strParamInfo, strParamClass, strXMLParamName, strParamName, 
                                                         strLength, strOptional, strRFCType, strCSTypeName, (nParamKind != 0));
                    strParamInfo = strParamInfo.Replace(REPLACE_ABAPNAME, strAbapName);
                    strClassSource = strClassSource.Replace(REPLACE_PARAM_INFO, strParamInfo);

                    // Server Proxy ������ ��� �Ʒ��� �Լ��� �������� �ʴ´�.
                    if (!bClientProxy) continue;

                    if (String.Equals(strParamClass, XML_VALUE_PARAMCLASS_E) 
                        || String.Equals(strParamClass, XML_VALUE_PARAMCLASS_T)
                        || String.Equals(strParamClass, "EI")
                        || String.Equals(strParamClass, "IE"))
                    {   // ������ ������ EXPORT �̸�, �Լ��� ��ȯ ����� �迭���� �� ������ ���� �����Ѵ�.
                        ++nOutOrder;
                        strResult = strResult + REPLACE_RESULT + "\n";
                        // �ּ��� �����Ѵ�.
                        strResult = strResult.Replace("//", "");

                        // ������ �̸��� �����Ѵ�.
                        strResult = strResult.Replace(REPLACE_PARAM_NAME, strParamName);

                        // ������ ������ �����Ѵ�.
                        strResult = strResult.Replace(REPLACE_VARTYPE, strCSTypeName);
                        // ��� ���տ� ���Ͽ�, ������ ������ �����Ѵ�.
                        strResult = strResult.Replace(REPLACE_PARAM_ORDER, nOutOrder.ToString());
                    }

                    if (!String.Equals(strParamClass, XML_VALUE_PARAMCLASS_E))
                    {	// ������ �̸��� �����Ѵ�.
                        strFuncParams = strFuncParams + strParamName + ",\n";
                    }
                } // for
                strClassSource = strClassSource.Replace(REPLACE_RESULT, strResult);
                //  �������� ��Ÿ���� Token("\n,") ���ڸ� �����Ѵ�.
                if (!String.IsNullOrEmpty(strFuncParams) && strFuncParams.EndsWith(strToken))
                    strFuncParams = strFuncParams.Remove(strFuncParams.Length - strToken.Length);
                strClassSource = strClassSource.Replace(REPLACE_PARAM_NAME, strFuncParams);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            return;
        }

        /// <summary>
        /// �ܼ� ������ ���� ������ �����Ͽ�, ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="strParamInfo">���� ����, ���ҽ� ���ø����� ���� ���ڿ�</param>
        /// <param name="strParamClass">������ Ŭ���� �̸�</param>
        /// <param name="strXMLParamName">XML ����ڿ��� ���Ǵ� ������ �̸�</param>
        /// <param name="strParamName">������ �̸�</param>
        /// <param name="strLength">����</param>
        /// <param name="strOptional">������ ���� ����</param>
        /// <param name="strRFCTypeName">RFC ������ ���� �̸�</param>
        /// <param name="strCSTypeName">C# ������ ���� �̸�</param>
        /// <param name="bComplex">�ܼ� ���� ����:false, ���̺� ���� ����:true</param>
        /// <returns>������ ���� ����</returns>
        private string GenParameterInfo(string strParamInfo, string strParamClass, string strXMLParamName, string strParamName, 
                                        string strLength, string strOptional, string strRFCTypeName, string strCSTypeName, 
                                        bool bComplex)
        {
            string strDirection = null; // ������ ����, "RFCINOUT.IN", "RFCINOUT.OUT", "RFCINOUT.INOUT", "NONE"
            // ======================= HARD-CODING SECTION =======================
            // ������ �̸��� �����Ѵ�.
            strParamInfo = strParamInfo.Replace(REPLACE_PARAM_NAME, strParamName);
            // XML ����ڿ��� �̸��� �����Ѵ�.
            strParamInfo = strParamInfo.Replace(REPLACE_XMLPARAMNAME, strXMLParamName); 

            strParamInfo = strParamInfo.Replace(REPLACE_RFCTYPE, strRFCTypeName);  // RFC ���� ���� �̸�
            // ������ ���̸� �����Ѵ�.
            strParamInfo = strParamInfo.Replace(REPLACE_INTLENGTH, strLength);
            strParamInfo = strParamInfo.Replace(REPLACE_INTLENGTH2, String.Format("{0}",Convert.ToInt32(strLength) * 2));

            // �ʼ� ������ �����Ѵ�.
            strParamInfo = String.Equals(strOptional, "") ? 
                strParamInfo.Replace(REPLACE_OPTIONAL, "false") :
                strParamInfo.Replace(REPLACE_OPTIONAL, "true");

            // ������ ������ �����Ѵ�.			
            switch (strParamClass)
            {
                case "I":
                    strDirection = "RFCINOUT.IN";
                    break;
                case "E":
                    strDirection = "RFCINOUT.OUT";
                    strCSTypeName = "out " + strCSTypeName;
                    break;
                case "T":
                case "C":
                case "EI":
                case "IE":
                    strDirection = "RFCINOUT.INOUT";
                    strCSTypeName = "ref " + strCSTypeName;
                    break;
                default:
                    strDirection = "RFCINOUT.NONE";
                    break;
            }
            strParamInfo = strParamInfo.Replace(REPLACE_DIRECTION, strDirection);

            // ������ �̸��� ������ �߰��Ѵ�.			
            strParamInfo = strParamInfo.Replace(REPLACE_VARTYPE, strCSTypeName);   // C#  ���� ���� �̸�
            // ======================= HARD-CODING SECTION =======================
            return strParamInfo;
        }

        /// <summary>
        /// Proxy Ŭ���� ���ø� �ڵ忡 Exception ���� ������ �߰��Ͽ� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="oNodeList">���� ������ ������ XML ��� ����Ʈ</param>
        /// <param name="strClassSource">Proxy Ŭ���� ���ø� �ڵ�</param>
        /// <returns>���� ������ �߰��� Ŭ���� ���ڿ�</returns>
        private void AddExceptParam2ProxyClass(XmlNodeList oNodeList, ref String strClassSource)
        {
            String strExceptName = null; // ���� ������ �̸�
            String strExceptParam = null; // ���� ����

            XmlNode oNode = null; // ���
            int nNodeCnt = 0;    // ����� ����

            try
            {
                nNodeCnt = oNodeList.Count;

                for (int i = 0; i < nNodeCnt; ++i)
                {
                    oNode = oNodeList.Item(i); // ��带 �����´�.

                    // ���ø� �ڵ��� String ���� ��ȯ�մϴ�.   
                    strExceptName = oNode.Attributes[XML_ATTRIBUTE_PARAMETER].Value;
                    strExceptName = Format1stUpper(strExceptName.ToLower());

                    // �ݺ� ������ �ʵ� ""���ø� + �ݺ� ����"���� ��ü�Ѵ�. ���� �ݺ��� ����....
                    strExceptParam = REPLACE_EXCEPT_INFO;
                    strExceptParam = strExceptParam.Replace(REPLACE_EXCEPT, strExceptName); // ���� �̸��� �ٲ۴�.
                    strExceptParam = strExceptParam.Replace("// ", ""); // �ּ��� �����Ѵ�.

                    strClassSource = strClassSource.Replace(REPLACE_EXCEPT_INFO, REPLACE_EXCEPT_INFO + "\n" + strExceptParam);
                } // for
            }
            catch (Exception exp)
            {
                throw exp;
            }
            return;
        }

        /// <summary>
        /// RFC Proxy ���� Ŭ������ �����Ѵ�.
        /// </summary>
        /// <param name="strNamespace">������ Ŭ������ ���ӽ����̽�</param>
        /// <param name="strClassName">Ŭ���� �̸�</param>
        /// <param name="strFuncName">��� RFC �Լ� �̸�</param>
        /// <param name="bClientProxy">Proxy ����: true - Client, false - Server</param>
        /// <param name="strInfoXML">RFC �Լ� ������ ���� �ִ� XML ����</param>
        /// <param name="bOutSource">�ҽ����� ��� ����</param>
        /// <param name="strOutPath">Ŭ������ ����� ���͸�</param>
        /// <returns>�Լ��� ����(true), ����(false)</returns>
        private bool gen_RFCProxyClass(string strNamespace, string strClassName, string strFuncName, 
                                       bool bClientProxy, string strInfoXML, bool bOutSource, string strOutPath)
        {
            XmlDocument oXMLDOC = null;   // ���� XML ���� ��ü
            XmlNodeList oNodeList = null; // ��� ����Ʈ
            XmlElement oXMLELEM = null;   // XML ���

            bool bResult = false;

            try
            {
                // Ŭ���� ��� ������  �ʱ�ȭ �Ѵ�.
                m_strFuncProxyClass = "";
                m_arrSAPStructClass.Clear();
                m_arrSAPTableClass.Clear();
                m_arrProcessedTableName.Clear();
                m_arrProcessedStructName.Clear();

                // ���� ������ DOM ��ü�� �ε��Ͽ�, Table ������ ���ڿ� ���Ͽ�, 
                // ���̺� ���� ���� ���ϰ� Collection ������ �����Ѵ�.
                if (!String.IsNullOrEmpty(strInfoXML))
                {
                    // XML ������ �ε��Ѵ�.
                    oXMLDOC = new XmlDocument();
                    oXMLDOC.PreserveWhitespace = false;
                    oXMLDOC.LoadXml(strInfoXML);

                    oXMLELEM = oXMLDOC.DocumentElement;

                    // X031LTable ��带 �����Ͽ�, ����ü Ŭ������ �����Ѵ�.
                    // XML �������� ���̺� ������ ���ڸ� ������ ��� ����Ʈ�� �˻��Ѵ�.
                    oNodeList = oXMLDOC.SelectNodes(XPATH_X031LROOT_NODE);
                    // ���̺� Ŭ������ �����Ѵ�. 
                    GenX031LStructTableProxy(oNodeList, strNamespace, bOutSource, strOutPath);
                }
                // �Լ� ȣ�� Ŭ������ �����Ѵ�.
                GenRFCFunctionProxy(oXMLELEM, strNamespace, strClassName, strFuncName, bClientProxy, bOutSource, strOutPath);

                bResult = true;
            }
            catch (Exception exp)
            {
                throw exp;
            }
            return bResult;
        }

        /// <summary>
        /// X031LTABLE ��忡�� Table / Struct ������ Ŭ������ �����Ѵ�.
        /// </summary>
        /// <param name="oNodeList">���̺� ������ X031LTABLE ��� ����Ʈ</param>
        /// <param name="strNamespace">������ Ŭ������ ���ӽ����̽�</param>
        /// <param name="bOutSource">�ҽ����� ��� ����</param>
        /// <param name="strOutPath">Ŭ������ ����� ���͸�</param>
        private void GenX031LStructTableProxy(XmlNodeList oNodeList, String strNamespace, bool bOutSource, String strOutPath)
        {
            XmlNode oTableNode = null;     // ���̺� ������ ���
            XmlNode oParentNode = null;    // ���̺� ���� ����� �θ� ���
            XmlNodeList oFldHNodeList = null;  // �ʵ��� ��� ����Ʈ

            String strTabName = null;   // ���̺� Ŭ���� �̸�
            String strStructName = null;// Struct Ŭ���� �̸�
            String strTypeName = null;  // ���̺� Ŭ������ ���� �̸�
            String strLength = null;    // Struct ����
            String strParamClass = null;// PARAMCLASS ��
            String strExid = null;      // EXID ��
            String strDtyp = null;      // DTYP ��
            String strRollName = null;  // X031LTable ����� ROLLNAME ��
            String strFieldName = null; // X031LTable ����� FIELDNAME ��

            int nInitialSize = 0; // �迭 ������ �ʱ� ��

            bool bSkipTable = false;  // Table Ŭ���� ó�� Flag
            bool bSkipStruct = false; // Struct Ŭ���� ó�� Flag

            // "ROOT_X031LTable" ��带 �����ϸ鼭 ó���Ѵ�.
            for (int i = 0; i < oNodeList.Count; ++i)
            {
                bSkipTable = false;  // Table Ŭ���� ó�� Flag �ʱ�ȭ
                bSkipStruct = false; // Struct Ŭ���� ó�� Flag �ʱ�ȭ
                strParamClass = "";// ParamClass ���� �ʱ�ȭ

                oTableNode = oNodeList[i];
                oParentNode = oTableNode.ParentNode;

                // ������ ��尡 �ڽ� ��带 ������ �ִ� ���, Structure, Table Ŭ������ �����Ѵ�.
                if (!oTableNode.HasChildNodes) continue;

                // ���� ����� ���� ��忡��......
                // EXID ���� ���Ѵ�.
                strExid = oParentNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                if (String.Equals(oParentNode.Name, "RFC_FUNINTTable"))
                {
                    // TABNAME ���� ���̺� Ŭ������ �̸�����...
                    strTabName = oParentNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                    // PARAMCLASS ���� ���Ѵ�.
                    strParamClass = oParentNode.Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                }
                else if (String.Equals(oParentNode.Name, "X031LTable"))
                {
                    // ROLLNAME ���� ���̺� Ŭ���� �̸�����...
                    strTabName = oParentNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                }

                // ������ "ROOT_X031LTable" ���� ����� "Tablen" ���� Structure ���̷� ���Ѵ�.
                strLength = oParentNode.SelectSingleNode(XPATH_X030LTABLE_NODE).Attributes[XML_ATTRIBUTE_TABLEN].Value;

                // ���� ����� �ڽ� ��忡��.......
                // TABNAME ���� Struct Ŭ������ �̸����� �����Ѵ�.
                strTypeName = strStructName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_TABNAME].Value;

                // �Ʒ��� ������ �����Ǹ�, Struct Ŭ������ �������� �ʴ´�.
                // 2. ���� ����� ������ 1���̴�.
                // 3. ROLLNAME ���� ""�̴�.
                // DTYP ���� �̿��Ͽ�, strTypeName ���� �ٽ� �����Ѵ�.
                if (oTableNode.ChildNodes.Count == 1) 
                {
                    // DTYP ���� ���Ѵ�.
                    strDtyp = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_DTYP].Value;
                    // �迭�� �ʱⰪ�� ���Ѵ�.
                    nInitialSize = Convert.ToInt32(oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_DBLENGTH].Value);
                    // ROLLNAME ���� "" �̸�....
                    strRollName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                    strFieldName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_FIELDNAME].Value;
                    bSkipStruct = String.IsNullOrEmpty(strRollName) && String.IsNullOrEmpty(strFieldName);
                    if (bSkipStruct)
                        strTypeName = ConvDTYP2Type(strDtyp);
                }

                //================================================================================================
                // 1. Table Class �ҽ��� �����Ѵ�.
                // ó���� TabName ������ Ȯ���Ѵ�.
                //  ó���� Ŭ���� �̸��� �˻��Ͽ�, �̹� ó���� ���̺� �̸��� ������ �Լ��� �����Ѵ�.
                for (int nCnt = 0; nCnt < m_arrProcessedTableName.Count; ++nCnt)
                {
                    bSkipTable = strTabName.Equals(Convert.ToString(m_arrProcessedTableName[nCnt]));
                    if (bSkipTable) break;  // ó���� �����̸�
                }
                if (!bSkipTable)
                {
                    if (!strParamClass.Equals("T") && (strExid.Equals("u") || strExid.Equals("v")))
                    {
                        // Nothing to do
                    }
                    else
                    {
                        // ó���� Ŭ���� �̸��� �߰��Ѵ�.
                        m_arrProcessedTableName.Add(strTabName);

                        // ���̺� �̸� ���� ������ ���� ������ ���̺� �̸��� "Table"�� �߰��Ѵ�.
                        if (String.Equals(strTabName, strTypeName)) strTabName += "Table";

                        // EXID ���� "u"�Ǵ� "v" �� ���, ���̺� Ŭ������ �������� �ʴ´�.
                        GenRFCTableCollectionClass(strNamespace, strTabName, strTypeName, nInitialSize, bOutSource, strOutPath);
                    }
                }
                //================================================================================================
                //  ó���� Ŭ���� �̸��� �˻��Ͽ�, �̹� ó���� ����ü �̸��� ������ �Լ��� �����Ѵ�.
                if (!bSkipStruct) // ������ SKIP �����̵Ǹ� ó������ �ʴ´�.
                {
                    for (int nCnt = 0; nCnt < m_arrProcessedStructName.Count; ++nCnt)
                    {
                        bSkipStruct = strStructName.Equals(Convert.ToString(m_arrProcessedStructName[nCnt]));
                        if (bSkipStruct) break;  // ó���� �����̸�
                    }
                }
                if (bSkipStruct) continue;
                
                // ó���� Ŭ���� �̸��� �߰��Ѵ�.
                m_arrProcessedStructName.Add(strStructName);

                // 2. Struct Class �ҽ��� �����Ѵ�.
                oFldHNodeList = oTableNode.ChildNodes;
                GenRFCStructClass(oFldHNodeList, strNamespace, strStructName, strLength, bOutSource, strOutPath);
                //================================================================================================
            } // For
        }

        /// <summary>
        /// DTYP ���� ���ڿ��� CS �������� ��ȯ�Ѵ�.
        /// </summary>
        /// <returns></returns>
        private String ConvDTYP2Type(String strDTYP)
        {
            String strCSType = null;

            switch (strDTYP)
            {
                case "CHAR":
                case "LANG":
                case "NUMC":
                case "STRG":
                case "SSTR":
                    strCSType = "String";
                    break;
                case "INT1":
                    strCSType = "Byte";
                    break;
                case "INT2":
                    strCSType = "short";
                    break;
                case "INT4":
                    strCSType = "int";
                    break;
                case "LRAW":
                case "RAW":
                case "RSTR":
                    strCSType = "Byte[]";
                    break;
                default:
                    break;
            }

            return strCSType;
        }

        public bool gen_RFCProxyDLL(string strNamespace, string strClassName, string strFuncName, bool bClientProxy,
                                    string strInfoXML, bool bOutSource, string strOutPath)
        {
            String[] strArrClass = null;    // �ҽ� �ڵ��� �迭
            int nIdx = 0;            // �ε���
            int nClassCnt = 0;       // �ҽ� Ŭ���� �ڵ��� �ε���
            int nResCode = 0;        // ������ ���

            String strFileName = null;         // ����� Proxy DLL ������ �̸�
            String strProxyDllFullName = null; // ����� Proxy DLL ������ ��ο� �̸�

            bool bResult = false;
            try
            {
                gen_RFCProxyClass(strNamespace, strClassName, strFuncName, bClientProxy, strInfoXML, bOutSource, strOutPath);

                // �迭�� ũ�⸦ �����Ѵ�.
                strArrClass = new String[m_arrSAPStructClass.Count + m_arrSAPTableClass.Count + 1];

                // ���̺� ����(SAPStructure) Ŭ���� ������ �Է� �ҽ� �迭�� �߰��Ѵ�.
                for (nIdx = 0; nIdx < m_arrSAPStructClass.Count; ++nIdx)
                {
                    strArrClass[nClassCnt] = (string)m_arrSAPStructClass[nIdx];
                    ++nClassCnt;
                }
                // ���̺��� ���� Ŭ����(SAPTable) ������ �Է� �ҽ� �迭�� �߰��Ѵ�.
                for (nIdx = 0; nIdx < m_arrSAPTableClass.Count; ++nIdx)
                {
                    strArrClass[nClassCnt] = (string)m_arrSAPTableClass[nIdx];
                    ++nClassCnt;
                }
                // RFC �Լ��� Proxy Ŭ������ �Է� �ҽ� �迭�� �߰��Ѵ�.
                strArrClass[nClassCnt] = m_strFuncProxyClass;

                // RFC �Լ��� �̸��� �̿��Ͽ�, ��� ������ �̸��� �����Ѵ�.
                strFileName = strFuncName.Replace("/", "_"); // ��� ���� �̸��� "/" ���� ������, "_"�� �����Ѵ�.
                strProxyDllFullName = strOutPath + strFileName + ".DLL";

                // SAP .NET Connector ������ ��θ� �����Ѵ�.
                nResCode = BuildRFCProxyDLL(strArrClass, strProxyDllFullName, bClientProxy);

                bResult = true;
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return bResult;
        }
    } // class
} // namaspace