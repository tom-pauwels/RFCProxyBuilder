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
    /// RFCProxyGen에 대한 요약 설명입니다.
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
        /// 리소스 매니저를 생성하여, 리소스에 연결한다.
        /// </summary>
        private void OpenResource()
        {
            String strResourceName = null;
            // Resource 이름을 생성. 솔루션에 "RFCProxyGen.resource" 파일이 반드시 있어야 한다.
            strResourceName = String.Format("{0}.{1}", this.GetType().Namespace, RESOURCE_FILENAME);

            // 리소스에 접근하기 위하여 관리자를 생성한다.
            if (m_oRM == null)
                m_oRM = new ResourceManager(strResourceName, this.GetType().Assembly);
        }

        /// <summary>
        /// 사용한 리소스를 해제한다.
        /// </summary>
        private void CloseResource()
        {
            if (m_oRM != null)
            {
                m_oRM.ReleaseAllResources();
                m_oRM = null;
            }
        }

        #region 형식 변환 함수

        /// <summary>
        /// EXID 값을 구조체 작성 단계에서 사용할 SAP RFC 데이터 형식, C# 데이터 형식 문자열로 변환한다.
        /// </summary>
        /// <param name="strEXIDVal">EXID 값</param>
        /// <param name="strRFCType">RFC 데이터 형식 문자열</param>
        /// <param name="strCSType">C# 데이터 형식 문자열</param>
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

        #endregion 형식 변환 함수

        // A typed collection of RFC_FIELDS elements.
        /// <summary>
        /// 지정된 형식의 테이블을 조작할 수 있도록 컬렉션 클래스를 만든다.
        /// </summary>
        /// <param name="strNamespace">네임스페이스 이름</param>
        /// <param name="strTableName">테이블 형식 문자열</param>
        /// <param name="nInitialSize">배열 변수의 초기 지정값</param>
        /// <param name="strTypeName">레코드 데이터 형식 문자열</param>
        /// <param name="bOutSource">소스 출력 여부</param>
        /// <param name="strOutputPath">출력 디렉터리</param>
        private void GenRFCTableCollectionClass(String strNamespace, String strTableName, String strTypeName, 
                                                int nInitialSize, bool bOutSource, String strOutputPath)
        {
            StreamWriter oSW = null;     // 스트림 작성
            String strFilePath = null;   // 파일 이름 및 경로
            String strClassSource = null;// 생성된 클래스 소스

            // 클래스 소스를 생성한다.
            try
            {
                // 이름에 "/" 값을 사용할 수 없으므로 "_"으로 대체한다.
                strFilePath = (bOutSource) ? strOutputPath + strTableName.Replace("/", "_") + ".CS" : null;

                // ======================= HARD-CODING SECTION =======================
                // 템플릿 코드의 String 값을 반환합니다.   
                // "RFCProxyGen.resource" 파일에 "RFC_TABLE_TEMP" 항목이 반드시 있어야 한다.
                strClassSource = m_oRM.GetString(RES_RFC_TABLE_TEMP);

                // 네임스페이스를 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_NAMESPACE, strNamespace);
                // 테이블의 형식 이름을 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_CLASSNAME, strTableName.Replace("/", "_"));
                // 레코드 형식 이름을 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_TYPENAME, strTypeName.Replace("/", "_"));
                // "return new String();" 문자열을 교체한다.
                strClassSource = strClassSource.Replace("new String();", "\"\";");
                // "return new Byte[]();" 문자열을 교체한다.
                strClassSource = strClassSource.Replace("return new Byte[]();", 
                                                String.Format("return new Byte[{0}];", nInitialSize));
                // ======================= HARD-CODING SECTION =======================

                if (!String.IsNullOrEmpty(strFilePath)) // 파일 이름이 지정되면
                {
                    // 파일 이름과 경로 설정
                    oSW = File.CreateText(strFilePath);
                    oSW.Write(strClassSource);

                }
                
                // 멤버에 생성된 클래스를 추가한다.
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
        /// RFC Table 구조 클래스 템플릿 소스 필드 정보를 추가하고, 두번째 길이를 반환한다.
        /// </summary>
        /// <param name="oFieldNodeList">필드 노드의 리스트</param>
        /// <param name="strClassSource">클래스 문자열</param>
        /// <returns>구조체의 두번째 길이, Length2</returns>
        private int AddFieldInfo2StructClass(XmlNodeList oFieldNodeList, ref String strClassSource)
        {
            XmlNode oFieldNode = null;      // 필드 노드
            String strElemFieldTemp = null; // XmlElement 형식의 필드
            String strArrayFieldTemp = null;// XmlArray 형식의 필드
            String strFieldSource = null;   // 필드의 소스

            String strEXIDVal = null;  // 필드 노드의 EXID 값
            String strAbapName = null; // ABAP Field Name
            String strRFCType = null;  // RFC 형식
            String strCSType = null;   // CS 형식

            String strDecimals = null; // 필드의 DECIMALS

            String strFieldDepth = null; // 첫번째 필드의 DEPTH 값
            String strDepth = null;   // 필드의 DEPTH 값
            String strLength = null; int nLength = 0;   // 필드의 길이
            String strOffset = null; int nOffset = 0;   // 필드의 Offset
            String strLength2 = null;int nLength2 = 0;  // 필드의 길이
            String strOffset2 = null;int nOffset2 = 0;  // 필드의 Offset

            int nStructLength2 = 0;   // 구조체의 크기

            try
            {
                // 리소스에서 필드 템플릿을 가져온다.
                strElemFieldTemp = m_oRM.GetString(RES_RFC_ELEMENTFIELD_TEMP);
                strArrayFieldTemp = m_oRM.GetString(RES_RFC_ARRAYFIELD_TEMP);

                // 필드의 처움 노드에서 DEPTH 값을 가져온다.
                if (oFieldNodeList.Count >= 1 )
                    strFieldDepth = oFieldNodeList[0].Attributes[XML_ATTRIBUTE_DEPTH].Value;

                // 필드 노드를 반복하여 처리한다.
                for (int i = 0; i < oFieldNodeList.Count; ++i)
                {
                    oFieldNode = oFieldNodeList[i];

                    // 필드 노드의 EXID 값이 "L"이면, 처리하지 않는다.
                    strEXIDVal = oFieldNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                    // EXID 값이 "L"이거나 ""이면 처리하지 않는다.
                    if (strEXIDVal.Equals("L") || strEXIDVal.Equals("")) continue;

                    // 필드 노드의 ABAP 이름을 가져온다.
                    strAbapName = oFieldNode.Attributes[XML_ATTRIBUTE_FIELDNAME].Value;
                    // 필드 노드의 DECIMALS 값을 가져온다.
                    strDecimals = oFieldNode.Attributes[XML_ATTRIBUTE_DECIMALS].Value;
                    // 필드 노드의 DEPTH 값을 가져온다.
                    strDepth = oFieldNode.Attributes[XML_ATTRIBUTE_DEPTH].Value;
                   
                    // 구조체의 길이와 OFFSET 위치를 구한다.
                    strLength = oFieldNode.Attributes[XML_ATTRIBUTE_DBLENGTH].Value;
                    strOffset = oFieldNode.Attributes[XML_ATTRIBUTE_OFFSET].Value;

                    // 이전 노드의 LENGTH2 값과 OFFSET2 길이를 더하여 현재 노드의 OFFSET2 값을 구한다.
                    if (String.Equals(strFieldDepth, strDepth))
                        nOffset2 += nLength2;

                    // 현재 노드의 값을 정수형으로 변환한다.
                    nLength = Convert.ToInt32(strLength);
                    nOffset = Convert.ToInt32(strOffset);
                    
                    // 현재 노드의 길이를 구한다.
                    // RFC(ABAP) 형태 중에서 .NET String 형태로 변환되는 형식
                    // [C (String)], [D (Date)], [T (Time)], [N (Numc)], [RFC String], [XString]
                    switch (strEXIDVal)
                    {
                        case "h":
                        case "g":
                        case "u": // Length2 값에 Length 값을 대입한다.
                            nLength2 = nLength;
                            break;
                        case "v":
                            // 나중에 점검 필요함.
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
                    // STRUCT_LENGTH 길이를  계산한다.
                    nStructLength2 += nLength2; // Offset 누적, 다음 위치를 계산한다.
   
                    // 첫번째 노드의 DEPTH 값과 현재 노드의 DEPTH 값을 비교하여, 값이 다르면 처리하지 않는다.
                    if (String.Equals(strFieldDepth, strDepth))
                    {
                        // 만일 현재 노드의 OFFSET 값이 OFFSET2 값보다 큰 경우, OFFSET2 값을 변경한다.
                        if (nOffset > nOffset2) nOffset2 = nOffset;
                        // STRUCT_LENGTH2 값에 보정 값을 더한다.
                        nStructLength2 += (nOffset2 % 2);
                        // OFFSET2 값을 짝수로 만든다.
                        nOffset2 += (nOffset2 % 2);
                        strOffset2 = nOffset2.ToString();
                    }
                    else
                    {
                        continue;
                    }

                    // 필드 노드의 EXID 값이 "h" 또는 "v" (RFCTYPE_XMLDATA 형식)이면,
                    // strArrayFieldTemp 형식을 사용한다 나머지 형식은 strElemFieldTemp
                    strRFCType = strCSType = null; // 참조 변수 초기화
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

                    // 필드 소스의 문자열을 교체한다.
                    ReplaceFieldSource(ref strFieldSource, strAbapName,
                                        strEXIDVal, strRFCType, strCSType, strDecimals,
                                        strLength, strLength2, strOffset, strOffset2);
                    // 클래스 소스의 문자열을 교체한다.
                    strClassSource = strClassSource.Replace(REPLACE_PARAM_INFO, strFieldSource + "\n" + REPLACE_PARAM_INFO);
                   
               } // for
            }
            catch (Exception exp)
            {
                throw exp;
            }
            // 구조체 크기를 반환한다.
            return nStructLength2;
        }

        /// <summary>
        /// 문자열을 교채한다.
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
                // EXID 값이 "u"이면 필드 소스에서 길이를 제거한다.
                if (strEXIDVal.Equals("u"))
                    strFieldSource = strFieldSource.Replace(STR_REMOVE_LENGTH, "");
                // DECIMALS 값이 0 이면 제거한다.
                if (strDecimals.Equals("0"))
                    strFieldSource = strFieldSource.Replace(STR_REMOVE_DECIMALS, "");

                strCShapType = (strCSType.Equals("")) ? strAbapName : strCSType;

                strCShapType = strCShapType.Replace("/", "_");
                strCShapType = strCShapType.Replace("%", "_");
                strCShapType = strCShapType.Replace("$", "_");

                // "/" 문자를 "_" 문자로, "%" 문자를 "_--25" 문자로 교체한다.
                strXMLName = strAbapName.Replace("/", "_");
                strXMLName = strXMLName.Replace("#", "_--23");
                strXMLName = strXMLName.Replace("$", "_--24");
                strXMLName = strXMLName.Replace("%", "_--25");
                strXMLName = strXMLName.Replace("&", "_--26");
                // strXMLName 값이 숫자로 시작하면, "_--3" 문자를 추가한다.
                if (Char.IsNumber(strXMLName, 0)) strXMLName = "_--3" + strXMLName;

                // "/" 문자를 "_" 문자로,"%" 문자를 "__" 문자로 교체한다.
                // CS 예약어와의 충돌을 피하기 위해 앞에 "_" 를 붙인다.
                strFieldName = strAbapName.Replace("#", "__");
                strFieldName = strFieldName.Replace("/", "_").ToLower();
                strFieldName = strFieldName.Replace("$", "_").ToLower();
                strFieldName = strFieldName.Replace("%", "__").ToLower();
                strFieldName = strFieldName.Replace("&", "_").ToLower();
                strFieldName = Format1stUpper(strFieldName);
                // strFieldName 값이 숫자로 시작하면, "N" 문자를 추가한다.
                if (Char.IsNumber(strFieldName, 0)) strFieldName = "N" + strFieldName;
                //strFieldName = "_" + strFieldName[0].ToString().ToUpper() + strFieldName.Substring(1);

                // 필드 소스를 수정한다.
                strFieldSource = strFieldSource.Replace(REPLACE_INTLENGTH2, strLength2);   // 두번째 필드 길이를 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_OFFSET2, strOffset2);      // 두번째 OFFSET 위치를 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_ABAPNAME, strAbapName);    // ABAP 이름을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPALCE_FIELDXMLNAME, strXMLName); // XML 요소 이름을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_FIELDNAME, strFieldName);  // 필드 이름을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_RFCTYPE, strRFCType);      // RFC 형식 이름을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_INTLENGTH, strLength);     // 필드 길이을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_DECIMALS, strDecimals);    // DECIMALS 값을 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_OFFSET, strOffset);        // offset 위치를 바꾼다.
                strFieldSource = strFieldSource.Replace(REPLACE_VARTYPE, strCShapType);    // C# 변수 형식 이름을 바꾼다.
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// Field 이름을 수정한다.
        /// </summary>
        /// <param name="strFieldName">원본 필드 이름</param>
        /// <returns>수정된 필드 이름</returns>
        private String Format1stUpper(String strFieldName)
        {
            int nPos = 0; // "_" 문자의 위치
            String strRetVal = null; // 수정된 필드의 이름
            Char[] chFieldName = null;

            chFieldName = strFieldName.ToCharArray();

            nPos = -1;
            do
            {
                // "_" 문자의 다음 위치의 문자를 대문자로 고친다.
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
        /// RFC 구조(Struct) 클래스를 만든다. 리소스(RFCProxyGen.resources)를 사용한다.
        /// </summary>
        /// <param name="oFieldNodeList">구조 정보를 가지고 있는 XML 문서의 노드 리스트</param>
        /// <param name="strNamespace">네임스페이스 이름</param>
        /// <param name="strAbapName">구조체 클래스의 ABAP 이름</param>
        /// <param name="strLength">구조체의 길이</param>
        /// <param name="bOutSource">소스 출력 여부</param>
        /// <param name="strOutPath">출력 디렉터리</param>
        /// <returns>생성된 클래스 문자열</returns>
        private void GenRFCStructClass(XmlNodeList oFieldNodeList, String strNamespace, String strAbapName,
                                       String strLength, bool bOutSource, string strOutPath)
        {
            String strClassSource = null;// RFC 구조 정보 클래스 문자열

            String strClassName = null;  // SAPStruct 클래스 이름
            String strFilePath = null;   // 출력 파일의 경로와 이름
            StreamWriter oSW = null;     // 스트림 작성

            int nLength2 = 0;            // 구조체의 Length2 값

            try
            {
                // "RFCProxyGen.resource" 파일에서 구조체 소스 템플릿을 로드한다.
                strClassSource = m_oRM.GetString(RES_RFC_STRUCTURE_TEMP);

                // 클래스 이름에 "/" 문자를 사용할 수 없으므로 "_" 문자로 교체한다.
                strClassName = strAbapName.Replace("/", "_");

                // 소스 템플릿의 문자열을 교체한다.
                strClassSource = strClassSource.Replace(REPLACE_NAMESPACE, strNamespace);  // 네임 스페이스를 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_ABAPNAME,  strAbapName);   // ABAP 테이블 이름을 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_CLASSNAME, strClassName);  // 클래스 이름을 바꾼다.
                strClassSource = strClassSource.Replace(REPLACE_INTLENGTH, strLength);     // 테이블 길이을 바꾼다.

                // 템플릿 코드에 필드 정보를 추가한다. 함수의 반환 값은 구조체의 Length2 길이다.
                nLength2 = AddFieldInfo2StructClass(oFieldNodeList, ref strClassSource);
                if (Convert.ToInt32(strLength) > nLength2) nLength2 = Convert.ToInt32(strLength);
                strClassSource = strClassSource.Replace(REPLACE_INTLENGTH2, nLength2.ToString());// 테이블 길이을 바꾼다.

                // 멤버에 생성된 클래스를 추가한다.
                m_arrSAPStructClass.Add(strClassSource);

                if (bOutSource)
                {
                    // 파일 이름과 경로 설정
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
        /// RFC 함수의 Proxy 클래스를 생성한다.
        /// </summary>
        /// <param name="oXmlElem">구조 정보를 가지고 있는 XML 문서 요소</param>
        /// <param name="strNamespace">네임스페이스 이름</param>
        /// <param name="strClassName">클래스 이름</param>
        /// <param name="strAbapName">ABAP 이름</param>
        /// <param name="bClientProxy">Proxy 형태, true:Client, false:Server</param>
        /// <param name="bOutSource">소스 파일 출력 여부</param>
        /// <param name="strOutPath">소스 파일 출력 경로</param>
        /// <returns>생성된 클래스 문자열</returns>
        private void GenRFCFunctionProxy(XmlElement oXmlElem, String strNamespace, String strClassName, String strAbapName,
                                         bool bClientProxy, bool bOutSource, String strOutPath)
        {
            String strProxyClass = null;  // 리소스에서 획득할 Proxy 클래스 Template
            XmlNodeList oNodeList = null; // 테이블 형식의 노드 리스트
            String strXmlPath = null;     // XML 문서 검색을 위한 XPATH 문자열

            String strProxyFuncName = null; // Proxy 호출 함수 이름
            String strProxyXMLName = null;  // Proxy XML Meta 정보에 사용될 함수 이름
            StreamWriter oSW = null;        // 스트림 작성
           
            String strProxyFileFullName = null;    // Proxy 파일의 경로와 이름

            try
            {
                // 서버, 클라이언트 형식에 맞는 소스 템플릿을 가져온다.
                strProxyClass = bClientProxy ? 
                                m_oRM.GetString(RES_CLIENT_PROXY_TEMP) : m_oRM.GetString(RES_SERVER_PROXY_TEMP);
                strProxyClass = strProxyClass.Replace(REPLACE_NAMESPACE, strNamespace); // 네임 스페이스를 바꾼다.
                strProxyClass = strProxyClass.Replace(REPLACE_CLASSNAME, strClassName); // 클래스 이름을 바꾼다.
                strProxyClass = strProxyClass.Replace(REPLACE_ABAPNAME,  strAbapName);  // RFC 함수(ABAP) 이름을 바꾼다.

                // 함수 이름이 영문으로 시작하면 앞에 "_" 문자를 붙인다.
                strProxyFuncName = (strAbapName[0] >= 0x30 && strAbapName[0] <= 0x39) ? "_" + strAbapName : strAbapName;
                // XML 헤더에 사용되는 이름을 교체한다.
                strProxyXMLName = strProxyFuncName.Replace("/", "_-");
                // 함수 이름에 "/" 값이 있으면 "_-" 문자로 교체한다.
                strProxyFuncName = strProxyFuncName.Replace("/", "_");
                // RFC Proxy 함수 이름과 XML 이름을 바꾼다.
                strProxyClass = strProxyClass.Replace(REPLACE_PROXYXMLNAME, strProxyXMLName);
                strProxyClass = strProxyClass.Replace(REPLACE_PROXYFUNCNAME, strProxyFuncName);

                if (oXmlElem != null)
                {
                    // XML 노드에서 Exception 관련 노드를 검색하여, 클래스 정보를 추가한다.
                    strXmlPath = XPATH_EXCEPTION_PARAM;
                    oNodeList = oXmlElem.SelectNodes(strXmlPath); // 노드 리스트를 가져온다.
                    if (oNodeList.Count > 0)
                        AddExceptParam2ProxyClass(oNodeList, ref strProxyClass);

                    // XML 노드에서 일반 인자(Exception 제외)의 노드를 검색하여, 클래스 정보를 추가한다.
                    strXmlPath = XPATH_NOT_EXCEPTION_PARAM;
                    oNodeList = oXmlElem.SelectNodes(strXmlPath); // 노드 리스트를 가져온다.
                    if (oNodeList.Count > 0)
                        AddNormalParam2ProxyClass(oNodeList, bClientProxy, ref strProxyClass);
                }

                // 출력 소스에서 Temp Line 제거
                strProxyClass = strProxyClass.Replace(REPLACE_EXCEPT_INFO, "");
                strProxyClass = strProxyClass.Replace(REPLACE_RESULT, "");
                strProxyClass = strProxyClass.Replace(REPLACE_PARAM_INFO, "");
                strProxyClass = strProxyClass.Replace(REPLACE_PARAM_NAME, "");
                
                // 멤버 변수에 생성된 클래스 소스를 대입한다.
                m_strFuncProxyClass = strProxyClass;
                
                if (bOutSource) // 파일 이름이 지정되면
                {
                    // RFC 함수의 Proxy 클래스를 생성한다.
                    strProxyFileFullName = strOutPath + strProxyFuncName + "Proxy.CS";

                    // 파일 이름과 경로 설정
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
        /// Proxy 클래스 템플릿 코드에 일반 인자 정보를 추가하여 반환한다.
        /// </summary>
        /// <param name="strSRC">Proxy 클래스 템플릿 코드</param>
        /// <param name="bClientProxy">Proxy 형태, true:Client, false:Server</param>
        /// <param name="strClassSource">클래스 소스 코드</param>
        /// <returns>일반 인자 정보가 추가된 클래스 문자열</returns>
        private void AddNormalParam2ProxyClass(XmlNodeList oNodeList, bool bClientProxy, ref String strClassSource)
        {
            String strSimpleParamTemp = null; // 기본, XmlElement 형식의 인자 정보
            String strTableParamTemp = null;  // XmlArray 형식의 인자 정보
            String strStructParamTemp = null; // 구조체 XmlElement 형식의 인자 정보
            
            String strParamInfo = null;  // 생성된 함수의 파라미터 선언부 정보
            String strFuncParams = null; // 함수의 실인자 정보

            #region 템플릿에서 교체될 문자열을 가진 변수
            String strAbapName = null;    // ABAP 이름, PARAMETER 노드의 값
            String strRFCType = null;     // RFC 형식, EXID 값을 이용하여 구한다.
            String strCSTypeName = null;  // C# 변수 형식 이름, EXID 값으로 기본 형식, XMLDATA, ITAB 형식은 TABNAME 값.
            String strOptional = null;    // OPTIONAL
            String strLength = null;      // INTLENGTH
            String strParamName = null;   // 파리미터 이름, ABAP 이름에서 변환된 형식 이름
            String strXMLParamName = null;// XML 요소에서 사용될 파라미터 이름
            String strParamClass = null;  // PARAMCLASS
            String strEXID = null;        // EXID
            String strResult = null; // Proxy 함수가 반환해야 할 결과 문자열 집합
            #endregion

            int nOutOrder = -1;   // 방향이 "OUT"으로 설정된 변수의 순서
            
            XmlNode oFieldNode = null;      // 파라미터 노드 RFC_FUNINTTable
            XmlNode oX031TableNode = null;  // 하위 필드의 노드
          
            String strX031TabName = null;   // X031Table 노드의 TABNAME 값
            String strX031RollName = null;  // X031Table 노드의 ROLLNAME 값
            bool bSameTabName = false;      // RFC_FUNINTTable 노드와 X031Table 노드의 TABNAME 값의 일치 여부
            int nX031NodeCnt = 0;           // X031Table 노드의 이름

            int nParamKind = 0;    // 인자의 종류, 0: Simple, 1: Table, 2: Structure
            ArrayList arProcessedParamName= null; // 처리된 파라미터 이름을 보관
            bool bProcessed = false;

            String strToken = ",\n"; // 반환될 결과 집합에서 사용할 토큰 문자열

            try
            {
                // 리소스에서 템플릿 코드를 가져온다.
                strSimpleParamTemp = m_oRM.GetString(RES_SIMPLE_PARA_TEMP);
                strTableParamTemp = m_oRM.GetString(RES_TABLE_PARA_TEMP);
                strStructParamTemp = m_oRM.GetString(RES_STRUCT_PARA_TEMP);

                // PARAMETER 이름이 같은 노드를 찾는다. 만일 이 노드들의 PARAMCLASS 값이 각각 "I", "E" 이면
                // ref 형식으로 선언하고, 출력 파라미터에 추가한다.

                arProcessedParamName = new ArrayList(oNodeList.Count);
                for (int i = 0; i < oNodeList.Count; ++i)
                {
                    strX031RollName = strX031TabName = null;
                    bSameTabName = false;
                    nX031NodeCnt = 0;

                    oFieldNode = oNodeList[i]; // 노드를 가져온다.
                    // 노드에서 값을 가져온다.
                    strParamClass = oFieldNode.Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                    strAbapName = oFieldNode.Attributes[XML_ATTRIBUTE_PARAMETER].Value;
                    strCSTypeName = oFieldNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                    strEXID = oFieldNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                    strLength = oFieldNode.Attributes[XML_ATTRIBUTE_INTLENGTH].Value;
                    strOptional = oFieldNode.Attributes[XML_ATTRIBUTE_OPTIONAL].Value;
                    // X031LTable 노드에서 TABNAME 값을 가져온다.
                    if (oFieldNode.HasChildNodes && oFieldNode.SelectSingleNode("." + XPATH_X031LROOT_NODE).HasChildNodes)
                    {
                        oX031TableNode = oFieldNode.SelectSingleNode("." + XPATH_X031LROOT_NODE).FirstChild;
                        strX031TabName = oX031TableNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                        strX031RollName = oX031TableNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;

                        bSameTabName = String.Equals(strCSTypeName, strX031TabName);
                        nX031NodeCnt = oFieldNode.FirstChild.ChildNodes.Count;
                    }

                    strXMLParamName = strParamName = strAbapName.Replace("/", "_"); // "/" 문자를 "_" 문자로 교체
                    if (Char.IsNumber(strParamName, 0)) // 첫 문자가 숫자로 시작하면,
                    {
                        // 파라미터 이름에 "N" 문자를 추가한다.
                        strParamName = "N" + strParamName;
                        // XML 파라미터 이름에 "_--3"을 추가한다.
                        strXMLParamName = "_--3" + strXMLParamName;
                    }
                    // 데이터 형식의 이름을 구한다.
                    ConvEXID2Type(strEXID, ref strRFCType, ref strCSTypeName);

                    #region 중복 처리 방지

                    // 파라미터 이름이 같은 노드들을 검사한다. 가끔 XML 정보에 같은 이름으로 표시되는 변수가 있다.
                    for (int j = i + 1; j < oNodeList.Count; ++j)
                    {
                        if (String.Equals(strAbapName,oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMETER].Value))
                        {
                            // strParamClass 값이 다르면 붙인다. 현재까지 최대 두글자로 "EI" 또는 "IE" 값으로 예상된다.
                            if (strParamClass.IndexOf(oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMCLASS].Value) < 0)
                                strParamClass += oNodeList[j].Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                        }
                    }
                    // 이미 처리한 이름의 파라미터가 있으면 처리하지 않는다.
                    bProcessed = false;
                    for (int j = 0; j < arProcessedParamName.Count; ++j)
                    {
                        if (!String.Equals(strParamName, (string)arProcessedParamName[j]))
                            continue;
                        
                        bProcessed = true;
                        break;
                    }
                    // 처리하지 않은 경우, 이름을 배열에 추가한다. 중복 방지.
                    if (!bProcessed)
                        arProcessedParamName.Add((Object)strParamName);
                    else
                        continue;
                    
                    #endregion 중복 처리 방지

                    nParamKind = 0;
                    
                    // PARAMCLASS = "T" 형식이거나, EXID 값이 아래와 같은 경우, Table 형태가 된다.
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
                            // 1. 상위 노드의 TABNAME 값과 하위 노드의 TABNAME 값이 같고    
                            // 2. 하위 노드의 갯수가 1 보다 크고
                            // 3. ROLLNAME 값이 ""이 아니면...
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

                    // 인자의 정보 문자열을 설정한다.
                    strCSTypeName = strCSTypeName.Replace("/", "_");// 형식 이름에 "/" 값을 포함 할 수 없으므로 "_" 값으로 교체한다.
                    
                    // 파라미터 정보를 생성한다.
                    strParamInfo = this.GenParameterInfo(strParamInfo, strParamClass, strXMLParamName, strParamName, 
                                                         strLength, strOptional, strRFCType, strCSTypeName, (nParamKind != 0));
                    strParamInfo = strParamInfo.Replace(REPLACE_ABAPNAME, strAbapName);
                    strClassSource = strClassSource.Replace(REPLACE_PARAM_INFO, strParamInfo);

                    // Server Proxy 형태인 경우 아래의 함수는 수행하지 않는다.
                    if (!bClientProxy) continue;

                    if (String.Equals(strParamClass, XML_VALUE_PARAMCLASS_E) 
                        || String.Equals(strParamClass, XML_VALUE_PARAMCLASS_T)
                        || String.Equals(strParamClass, "EI")
                        || String.Equals(strParamClass, "IE"))
                    {   // 인자의 방향이 EXPORT 이면, 함수의 반환 결과의 배열에서 그 인자의 값을 설정한다.
                        ++nOutOrder;
                        strResult = strResult + REPLACE_RESULT + "\n";
                        // 주석을 제거한다.
                        strResult = strResult.Replace("//", "");

                        // 인자의 이름을 설정한다.
                        strResult = strResult.Replace(REPLACE_PARAM_NAME, strParamName);

                        // 변수의 형식을 설정한다.
                        strResult = strResult.Replace(REPLACE_VARTYPE, strCSTypeName);
                        // 결과 집합에 대하여, 변수의 차수를 설정한다.
                        strResult = strResult.Replace(REPLACE_PARAM_ORDER, nOutOrder.ToString());
                    }

                    if (!String.Equals(strParamClass, XML_VALUE_PARAMCLASS_E))
                    {	// 인자의 이름을 설정한다.
                        strFuncParams = strFuncParams + strParamName + ",\n";
                    }
                } // for
                strClassSource = strClassSource.Replace(REPLACE_RESULT, strResult);
                //  마지막에 나타나는 Token("\n,") 문자를 제거한다.
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
        /// 단순 형식의 인자 정보를 생성하여, 반환한다.
        /// </summary>
        /// <param name="strParamInfo">인자 정보, 리소스 템플릿에서 구한 문자열</param>
        /// <param name="strParamClass">인자의 클래스 이름</param>
        /// <param name="strXMLParamName">XML 기술자에서 사용되는 인자의 이름</param>
        /// <param name="strParamName">인자의 이름</param>
        /// <param name="strLength">길이</param>
        /// <param name="strOptional">선택적 변수 여부</param>
        /// <param name="strRFCTypeName">RFC 데이터 형식 이름</param>
        /// <param name="strCSTypeName">C# 데이터 형식 이름</param>
        /// <param name="bComplex">단순 구조 형식:false, 테이블 구조 형식:true</param>
        /// <returns>생성된 인자 정보</returns>
        private string GenParameterInfo(string strParamInfo, string strParamClass, string strXMLParamName, string strParamName, 
                                        string strLength, string strOptional, string strRFCTypeName, string strCSTypeName, 
                                        bool bComplex)
        {
            string strDirection = null; // 인자의 방향, "RFCINOUT.IN", "RFCINOUT.OUT", "RFCINOUT.INOUT", "NONE"
            // ======================= HARD-CODING SECTION =======================
            // 인자의 이름을 설정한다.
            strParamInfo = strParamInfo.Replace(REPLACE_PARAM_NAME, strParamName);
            // XML 기술자에서 이름을 수정한다.
            strParamInfo = strParamInfo.Replace(REPLACE_XMLPARAMNAME, strXMLParamName); 

            strParamInfo = strParamInfo.Replace(REPLACE_RFCTYPE, strRFCTypeName);  // RFC 변수 형식 이름
            // 인자의 길이를 설정한다.
            strParamInfo = strParamInfo.Replace(REPLACE_INTLENGTH, strLength);
            strParamInfo = strParamInfo.Replace(REPLACE_INTLENGTH2, String.Format("{0}",Convert.ToInt32(strLength) * 2));

            // 필수 유무를 설정한다.
            strParamInfo = String.Equals(strOptional, "") ? 
                strParamInfo.Replace(REPLACE_OPTIONAL, "false") :
                strParamInfo.Replace(REPLACE_OPTIONAL, "true");

            // 인자의 방향을 설정한다.			
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

            // 형식의 이름에 방향을 추가한다.			
            strParamInfo = strParamInfo.Replace(REPLACE_VARTYPE, strCSTypeName);   // C#  변수 형식 이름
            // ======================= HARD-CODING SECTION =======================
            return strParamInfo;
        }

        /// <summary>
        /// Proxy 클래스 템플릿 코드에 Exception 인자 정보를 추가하여 반환한다.
        /// </summary>
        /// <param name="oNodeList">예외 정보를 포함한 XML 노드 리스트</param>
        /// <param name="strClassSource">Proxy 클래스 템플릿 코드</param>
        /// <returns>예외 정보가 추가된 클래스 문자열</returns>
        private void AddExceptParam2ProxyClass(XmlNodeList oNodeList, ref String strClassSource)
        {
            String strExceptName = null; // 예외 변수의 이름
            String strExceptParam = null; // 예외 정보

            XmlNode oNode = null; // 노드
            int nNodeCnt = 0;    // 노드의 갯수

            try
            {
                nNodeCnt = oNodeList.Count;

                for (int i = 0; i < nNodeCnt; ++i)
                {
                    oNode = oNodeList.Item(i); // 노드를 가져온다.

                    // 템플릿 코드의 String 값을 반환합니다.   
                    strExceptName = oNode.Attributes[XML_ATTRIBUTE_PARAMETER].Value;
                    strExceptName = Format1stUpper(strExceptName.ToLower());

                    // 반복 구문을 필드 ""템플릿 + 반복 구문"으로 대체한다. 다음 반복을 위해....
                    strExceptParam = REPLACE_EXCEPT_INFO;
                    strExceptParam = strExceptParam.Replace(REPLACE_EXCEPT, strExceptName); // 예외 이름을 바꾼다.
                    strExceptParam = strExceptParam.Replace("// ", ""); // 주석을 해제한다.

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
        /// RFC Proxy 관련 클래스를 생성한다.
        /// </summary>
        /// <param name="strNamespace">생성할 클래스의 네임스페이스</param>
        /// <param name="strClassName">클래스 이름</param>
        /// <param name="strFuncName">대상 RFC 함수 이름</param>
        /// <param name="bClientProxy">Proxy 종류: true - Client, false - Server</param>
        /// <param name="strInfoXML">RFC 함수 정보를 갖고 있는 XML 문서</param>
        /// <param name="bOutSource">소스파일 출력 여부</param>
        /// <param name="strOutPath">클래스를 출력할 디렉터리</param>
        /// <returns>함수의 성공(true), 실패(false)</returns>
        private bool gen_RFCProxyClass(string strNamespace, string strClassName, string strFuncName, 
                                       bool bClientProxy, string strInfoXML, bool bOutSource, string strOutPath)
        {
            XmlDocument oXMLDOC = null;   // 원본 XML 문서 개체
            XmlNodeList oNodeList = null; // 노드 리스트
            XmlElement oXMLELEM = null;   // XML 요소

            bool bResult = false;

            try
            {
                // 클래스 멤버 변수를  초기화 한다.
                m_strFuncProxyClass = "";
                m_arrSAPStructClass.Clear();
                m_arrSAPTableClass.Clear();
                m_arrProcessedTableName.Clear();
                m_arrProcessedStructName.Clear();

                // 인자 정보를 DOM 개체에 로드하여, Table 형식의 인자에 대하여, 
                // 테이블 구조 형식 파일과 Collection 파일을 생성한다.
                if (!String.IsNullOrEmpty(strInfoXML))
                {
                    // XML 문서를 로드한다.
                    oXMLDOC = new XmlDocument();
                    oXMLDOC.PreserveWhitespace = false;
                    oXMLDOC.LoadXml(strInfoXML);

                    oXMLELEM = oXMLDOC.DocumentElement;

                    // X031LTable 노드를 참조하여, 구조체 클래스를 생성한다.
                    // XML 문서에서 테이블 형식의 인자를 포함한 노드 리스트를 검색한다.
                    oNodeList = oXMLDOC.SelectNodes(XPATH_X031LROOT_NODE);
                    // 테이블 클래스를 생성한다. 
                    GenX031LStructTableProxy(oNodeList, strNamespace, bOutSource, strOutPath);
                }
                // 함수 호출 클래스를 생성한다.
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
        /// X031LTABLE 노드에서 Table / Struct 형태의 클래스를 생성한다.
        /// </summary>
        /// <param name="oNodeList">테이블 형식의 X031LTABLE 노드 리스트</param>
        /// <param name="strNamespace">생성할 클래스의 네임스페이스</param>
        /// <param name="bOutSource">소스파일 출력 여부</param>
        /// <param name="strOutPath">클래스를 출력할 디렉터리</param>
        private void GenX031LStructTableProxy(XmlNodeList oNodeList, String strNamespace, bool bOutSource, String strOutPath)
        {
            XmlNode oTableNode = null;     // 테이블 형식의 노드
            XmlNode oParentNode = null;    // 테이블 형식 노드의 부모 노드
            XmlNodeList oFldHNodeList = null;  // 필드의 노드 리스트

            String strTabName = null;   // 테이블 클래스 이름
            String strStructName = null;// Struct 클래스 이름
            String strTypeName = null;  // 테이블 클래스의 형식 이름
            String strLength = null;    // Struct 길이
            String strParamClass = null;// PARAMCLASS 값
            String strExid = null;      // EXID 값
            String strDtyp = null;      // DTYP 값
            String strRollName = null;  // X031LTable 노드의 ROLLNAME 값
            String strFieldName = null; // X031LTable 노드의 FIELDNAME 값

            int nInitialSize = 0; // 배열 변수의 초기 값

            bool bSkipTable = false;  // Table 클래스 처리 Flag
            bool bSkipStruct = false; // Struct 클래스 처리 Flag

            // "ROOT_X031LTable" 노드를 순항하면서 처리한다.
            for (int i = 0; i < oNodeList.Count; ++i)
            {
                bSkipTable = false;  // Table 클래스 처리 Flag 초기화
                bSkipStruct = false; // Struct 클래스 처리 Flag 초기화
                strParamClass = "";// ParamClass 값을 초기화

                oTableNode = oNodeList[i];
                oParentNode = oTableNode.ParentNode;

                // 현재의 노드가 자식 노드를 가지고 있는 경우, Structure, Table 클래스를 생성한다.
                if (!oTableNode.HasChildNodes) continue;

                // 현재 노드의 상위 노드에서......
                // EXID 값을 구한다.
                strExid = oParentNode.Attributes[XML_ATTRIBUTE_EXID].Value;
                if (String.Equals(oParentNode.Name, "RFC_FUNINTTable"))
                {
                    // TABNAME 값을 테이블 클래스의 이름으로...
                    strTabName = oParentNode.Attributes[XML_ATTRIBUTE_TABNAME].Value;
                    // PARAMCLASS 값을 구한다.
                    strParamClass = oParentNode.Attributes[XML_ATTRIBUTE_PARAMCLASS].Value;
                }
                else if (String.Equals(oParentNode.Name, "X031LTable"))
                {
                    // ROLLNAME 값을 테이블 클래스 이름으로...
                    strTabName = oParentNode.Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                }

                // 인접한 "ROOT_X031LTable" 형제 노드의 "Tablen" 값을 Structure 길이로 정한다.
                strLength = oParentNode.SelectSingleNode(XPATH_X030LTABLE_NODE).Attributes[XML_ATTRIBUTE_TABLEN].Value;

                // 현재 노드의 자식 노드에서.......
                // TABNAME 값을 Struct 클래스의 이름으로 정의한다.
                strTypeName = strStructName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_TABNAME].Value;

                // 아래의 조건이 만족되면, Struct 클래스는 생성하지 않는다.
                // 2. 하위 노드의 갯수가 1개이다.
                // 3. ROLLNAME 값이 ""이다.
                // DTYP 값을 이용하여, strTypeName 값을 다시 설정한다.
                if (oTableNode.ChildNodes.Count == 1) 
                {
                    // DTYP 값을 구한다.
                    strDtyp = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_DTYP].Value;
                    // 배열의 초기값을 구한다.
                    nInitialSize = Convert.ToInt32(oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_DBLENGTH].Value);
                    // ROLLNAME 값이 "" 이면....
                    strRollName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_ROLLNAME].Value;
                    strFieldName = oTableNode.ChildNodes[0].Attributes[XML_ATTRIBUTE_FIELDNAME].Value;
                    bSkipStruct = String.IsNullOrEmpty(strRollName) && String.IsNullOrEmpty(strFieldName);
                    if (bSkipStruct)
                        strTypeName = ConvDTYP2Type(strDtyp);
                }

                //================================================================================================
                // 1. Table Class 소스를 생성한다.
                // 처리된 TabName 값인지 확인한다.
                //  처리된 클래스 이름을 검색하여, 이미 처리된 테이블 이름이 있으면 함수를 종료한다.
                for (int nCnt = 0; nCnt < m_arrProcessedTableName.Count; ++nCnt)
                {
                    bSkipTable = strTabName.Equals(Convert.ToString(m_arrProcessedTableName[nCnt]));
                    if (bSkipTable) break;  // 처리된 상태이면
                }
                if (!bSkipTable)
                {
                    if (!strParamClass.Equals("T") && (strExid.Equals("u") || strExid.Equals("v")))
                    {
                        // Nothing to do
                    }
                    else
                    {
                        // 처리된 클래스 이름을 추가한다.
                        m_arrProcessedTableName.Add(strTabName);

                        // 테이블 이름 값과 형식의 값이 같으면 테이블 이름에 "Table"을 추가한다.
                        if (String.Equals(strTabName, strTypeName)) strTabName += "Table";

                        // EXID 값이 "u"또는 "v" 인 경우, 테이블 클래스는 생성하지 않는다.
                        GenRFCTableCollectionClass(strNamespace, strTabName, strTypeName, nInitialSize, bOutSource, strOutPath);
                    }
                }
                //================================================================================================
                //  처리된 클래스 이름을 검색하여, 이미 처리된 구조체 이름이 있으면 함수를 종료한다.
                if (!bSkipStruct) // 위에서 SKIP 설정이되면 처리하지 않는다.
                {
                    for (int nCnt = 0; nCnt < m_arrProcessedStructName.Count; ++nCnt)
                    {
                        bSkipStruct = strStructName.Equals(Convert.ToString(m_arrProcessedStructName[nCnt]));
                        if (bSkipStruct) break;  // 처리된 상태이면
                    }
                }
                if (bSkipStruct) continue;
                
                // 처리된 클래스 이름을 추가한다.
                m_arrProcessedStructName.Add(strStructName);

                // 2. Struct Class 소스를 생성한다.
                oFldHNodeList = oTableNode.ChildNodes;
                GenRFCStructClass(oFldHNodeList, strNamespace, strStructName, strLength, bOutSource, strOutPath);
                //================================================================================================
            } // For
        }

        /// <summary>
        /// DTYP 형식 문자열을 CS 형식으로 변환한다.
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
            String[] strArrClass = null;    // 소스 코드의 배열
            int nIdx = 0;            // 인덱스
            int nClassCnt = 0;       // 소스 클래스 코드의 인덱스
            int nResCode = 0;        // 컴파일 결과

            String strFileName = null;         // 출력할 Proxy DLL 파일의 이름
            String strProxyDllFullName = null; // 출력할 Proxy DLL 파일의 경로와 이름

            bool bResult = false;
            try
            {
                gen_RFCProxyClass(strNamespace, strClassName, strFuncName, bClientProxy, strInfoXML, bOutSource, strOutPath);

                // 배열의 크기를 설정한다.
                strArrClass = new String[m_arrSAPStructClass.Count + m_arrSAPTableClass.Count + 1];

                // 테이블 구조(SAPStructure) 클래스 파일을 입력 소스 배열에 추가한다.
                for (nIdx = 0; nIdx < m_arrSAPStructClass.Count; ++nIdx)
                {
                    strArrClass[nClassCnt] = (string)m_arrSAPStructClass[nIdx];
                    ++nClassCnt;
                }
                // 테이블을 조작 클래스(SAPTable) 파일을 입력 소스 배열에 추가한다.
                for (nIdx = 0; nIdx < m_arrSAPTableClass.Count; ++nIdx)
                {
                    strArrClass[nClassCnt] = (string)m_arrSAPTableClass[nIdx];
                    ++nClassCnt;
                }
                // RFC 함수의 Proxy 클래스를 입력 소스 배열에 추가한다.
                strArrClass[nClassCnt] = m_strFuncProxyClass;

                // RFC 함수의 이름을 이용하여, 출력 파일의 이름을 설정한다.
                strFileName = strFuncName.Replace("/", "_"); // 출력 파일 이름에 "/" 값이 있으면, "_"로 변경한다.
                strProxyDllFullName = strOutPath + strFileName + ".DLL";

                // SAP .NET Connector 파일의 경로를 설정한다.
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