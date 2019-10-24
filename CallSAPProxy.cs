using System;
using System.Data;
using SAP.Connector;
using SAP.Connector.Rfc;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace RFCProxyBuilder
{
    /// <summary>
    /// CallSAPProxy에 대한 요약 설명입니다.
    /// </summary>
    public class CallSAPProxy : SAPServerConnector
    {
        #region Member VARIABLE
        private const String COLUMN_PARAMETER = "PARAMETER";
        private const String COLUMN_PARAMCLASS = "PARAMCLASS";
        private const String COLUMN_FIELDNAME = "FIELDNAME";
        private const String COLUMN_EXID = "EXID";
        private const String COLUMN_TABNAME = "TABNAME";
        private const String COLUMN_TABTYPE = "ROLLNAME";
        private const String XPATH_HNODE = "//*[(@EXID = 'h' or @EXID = 'v' or @EXID = 'u') and (count(.//child::*) <= 0)]";
        #endregion

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public CallSAPProxy()
        {
        }

        /// <summary>
        /// 기본 소멸자
        /// </summary>
        ~CallSAPProxy()
        {
            base.DisconnectSAPServer();
        }

        /// <summary>
        ///  SAP 서버에 연결한다.
        /// </summary>
        /// <returns></returns>
        public override bool ConnectSAPServer()
        {
            return base.ConnectSAPServer();
        }

        /// <summary>
        ///  SAP 서버에 연결한다.
        /// </summary>
        /// <returns></returns>
        private bool ConnectSAPServer(string strTYPE, string strASHOST, short nSYSNR, short nCLIENT, string strLANG, string strUSER, string strPASSWD)
        {
            bool bRetVal = false;

            base.SetConnectionInfo(strTYPE, strASHOST, nSYSNR, nCLIENT, strLANG, strUSER, strPASSWD); // 환경 정보를 설정한다.
            bRetVal = base.ConnectSAPServer(); // SAP 서버에 연결한다.

            return bRetVal;
        }

        /// <summary>
        /// SAP 서버에서 연결을 해제한다.
        /// </summary>
        public override void DisconnectSAPServer()
        {
            base.DisconnectSAPServer();
        }

        /// <summary>
        /// RFC 함수 이름을 검색하여, 그 결과를 반환한다. SAP 서버에 연결되지 않은 경우, null 값을 반환.
        /// </summary>
        /// <param name="strFuncName">함수 이름 또는 필터</param>
        /// <param name="strGroupName">그룹 이름</param>
        /// <param name="strLang">언어</param>
        /// <returns>SAP 함수 검색 결과 테이블 (ADO.NET DataTable)</returns>
        private DataTable RFC_Function_Search(string strFuncName, string strGroupName, string strLang)
        {
            RFCFUNCTable oRfcTbl = null;

            // SAP 서버에 연결되지 않은 경우, null 값을 반환하고 함수를 종료한다.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oRfcTbl = new RFCFUNCTable();

                RFC_FUNCTION_SEARCH(strFuncName, strGroupName, strLang, ref oRfcTbl);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oRfcTbl.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }

        /// <summary>
        /// 함수의 인자 정보를 반환한다. SAP 서버에 연결되지 않은 경우, null 값을 반환.
        /// </summary>
        /// <param name="strFuncName">함수 이름</param>
        /// <param name="strLang">언어</param>
        /// <returns>SAP 함수의 인자 정보 (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Function_Interface(string strFuncName, string strLang)
        {
            RFC_FUNINTTable oRFCFT = null; // RFC_FUNINT 클래스

            // SAP 서버에 연결되지 않은 경우, null 값을 반환하고 함수를 종료한다.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oRFCFT = new RFC_FUNINTTable();

                // none_unicode_length => 공백과 'X'를 사용
                // 공백일 경우엔 unicode 길이를, 'X'일 경우엔 non-unicode 파라미터 길이를 반환함.
                RFC_GET_FUNCTION_INTERFACE(strFuncName, strLang, "X", ref oRFCFT);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oRFCFT.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }

        /// <summary>
        /// RFC 구조체(테이블) 형태의 정의 내역을 반환한다. SAP 서버에 연결되지 않은 경우, null 값을 반환
        /// </summary>
        /// <param name="strTabName">구조체(테이블) 이름</param>
        /// <param name="nTabLen">구조체(테이블) 길이</param>
        /// <returns>RFC 구조체(테이블) 형태의 정의 내역 (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Structure_Definition(string strTabName, out int nTabLen)
        {
            RFC_FIELDSTable oRFCFT = null;
            nTabLen = -1;

            // SAP 서버에 연결되지 않은 경우, null 값을 반환하고 함수를 종료한다.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oRFCFT = new RFC_FIELDSTable();

                RFC_GET_STRUCTURE_DEFINITION(strTabName, null, out nTabLen, ref oRFCFT);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oRFCFT.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }

        /// <summary>
        /// 유니코드 형태의 RFC 구조체(테이블) 형태의 정의 내역을 반환한다. SAP 서버에 연결되지 않은 경우, null 값을 반환
        /// </summary>
        /// <param name="strTabName">구조체(테이블) 이름</param>
        /// <param name="nLen1">구조체(테이블) 길이1</param>
        /// <param name="nLen2">구조체(테이블) 길이2</param>
        /// <returns>RFC 구조체(테이블) 형태의 정의 내역 (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Unicode_Structure(string strTabName, out int nLen1, out int nLen2)
        {
            int nB1TabLen = 0;
            int nB2TabLen = 0;
            int nB4TabLen = 0;
            int nCharLen = 0;
            byte[] btUUID = null;

            RFC_FLDS_UTable oFields = null;

            nLen1 = nLen2 = -1;

            // SAP 서버에 연결되지 않은 경우, null 값을 반환하고 함수를 종료한다.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oFields = new RFC_FLDS_UTable();

                RFC_GET_UNICODE_STRUCTURE("", "", "", strTabName, out nB1TabLen, out nB2TabLen, out nB4TabLen, out nCharLen, out btUUID, ref oFields);

                nLen1 = nB1TabLen;
                nLen2 = nB2TabLen;
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oFields.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }


        /// <summary>
        /// 테이블 정보
        /// </summary>
        /// <param name="strTabName">테이블 이름</param>
        /// <param name="oHeader"> ADO.NET DataTable of Database Structure DDNTT</param>
        /// <returns>RFC 구조체(테이블) 형태의 정의 내역 (ADO.NET DataTable of X031L elements)</returns>
        private DataTable RFC_Get_TabName(String strTabName, out X030L oHeader)
        {
			X031LTable oNameTab  = null;
            oHeader = null;

            // SAP 서버에 연결되지 않은 경우, null 값을 반환하고 함수를 종료한다.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oNameTab = new X031LTable();
                RFC_GET_NAMETAB(strTabName, out oHeader, ref oNameTab);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oNameTab.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }
        /// <summary>
        /// DataTable 개체에서 XML 문서를 생성하여 반환한다.
        /// </summary>
        /// <param name="oDT">DataTable 개체</param>
        /// <returns>XML 문서</returns>
        private String ConvertDataTable2XML(DataTable oDT)
        {
            String strXML = null;       // 반환할 XML 문서
            DataSet oDS = null;         // 테이블에서 XML 문서를 생성하기 위해 필요
            String strNamespace = null; // 제거할 네임스페이스
            int nColIdx = 0; // Column Index
            int nRowIdx = 0; // Row Index

            DataTable oDTClone = null; // Clone DataTable 
            DataRow oRow = null; // // Clone Record
            try
            {
                // Attribute 위주의 XML 문서를 생성하기 위하여, Table 속성을 변경한다.
                for (nColIdx = 0; nColIdx < oDT.Columns.Count; ++nColIdx)
                    oDT.Columns[nColIdx].ColumnMapping = MappingType.Attribute;

                // "X030LTable", "X031LTable" 테이블에 대한 작업을 수행한다.
                if (String.Equals(oDT.TableName, "X031LTable") || String.Equals(oDT.TableName, "X030LTable"))
                {
                    // Clone Table 생성
                    oDTClone = oDT.Clone();
                    // Clone Table 열의 형식을 변경한다.
                    for (nColIdx = 0; nColIdx < oDTClone.Columns.Count; ++nColIdx)
                    {
                        // Byte[] 형식의 열의 순차를 저장한다.
                        if (oDTClone.Columns[nColIdx].DataType.Equals(Type.GetType("System.Byte[]")))
                            oDTClone.Columns[nColIdx].DataType = Type.GetType("System.Int32");
                    } // for

                    // 원본 Table 데이터를 변환하여 Clone 테이블로 복사한다.
                    for (nRowIdx = 0; nRowIdx < oDT.Rows.Count; ++nRowIdx)
                    {
                        oRow = oDTClone.NewRow();

                        for (nColIdx = 0; nColIdx < oDT.Columns.Count; ++nColIdx)
                        {
                            if (oDT.Columns[nColIdx].DataType.Equals(Type.GetType("System.Byte[]")))
                            {
                                oRow[nColIdx] = ConvertByte2Int((Byte[])oDT.Rows[nRowIdx][nColIdx]);
                            }
                            else
                            {
                                oRow[nColIdx] = oDT.Rows[nRowIdx][nColIdx];
                            }
                        }
                        oDTClone.Rows.Add(oRow);
                    } //outer for 
                    oDTClone.AcceptChanges();
                } // if

                oDS = new DataSet(); // DataSet 개체를 생성
                if (oDTClone != null)
                    oDS.Tables.Add(oDTClone); // 테이블을 추가.
                else
                    oDS.Tables.Add(oDT); // 테이블을 추가.
                // 테이블 이름으로 ROOT 노드의 이름을 설정한다.
                oDS.DataSetName = "ROOT_" + oDT.TableName;

                //Load the document with the DataSet.
                XmlDataDocument oXMLDOC = new XmlDataDocument(oDS);

                // XML 문서를 획득한다.
                strXML = oXMLDOC.InnerXml;

                // 현재 프로그램의 네임스페이스를 획득한다.
                strNamespace = String.Format("xmlns=\"{0}\"", this.GetType().Namespace);

                // XML 문서에서 네임스페이스와 불필요한 정보를 제거한다.
                strXML = strXML.Replace(strNamespace, "");
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return strXML;
        }

        /// <summary>
        /// 256 진수의 바이트 배열에 대한 변환을 수행한다.
        /// </summary>
        /// <param name="bt256">256 진수의 바이트 배열</param>
        /// <returns>변환된 10진수 정수</returns>
        private int ConvertByte2Int(Byte[] bt256)
        {
            Double dblRetVal = 0;
            int nSize = bt256.Length - 1; // 256 진수의 바이트 배열 인덱스

            for (int i = nSize; i >= 0; --i)
            {
                dblRetVal += ((Double)bt256[i] * Math.Pow(0x100, nSize - i));
            }

            return (int)dblRetVal;
        }

        /// <summary>
        /// RFC 함수 이름을 검색하여, 그 결과를 반환한다. SAP 서버에 연결되지 않은 경우, null 값을 반환.
        /// </summary>
        /// <param name="strFilter">함수 이름 또는 필터</param>
        /// <param name="strGroupName">그룹 이름</param>
        /// <param name="strLang">언어</param>
        /// <returns>SAP 함수 검색 결과 테이블 (ADO.NET DataTable)</returns>
        public DataTable get_RFCFunctionNames(string strFilter, string strGroupName, string strLang)
        {
            DataTable oDT = null;   // 데이터 테이블 개체			
            try
            {
                oDT = RFC_Function_Search(strFilter, strGroupName, strLang); // RFC 함수 호출
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oDT;
        }

        /// <summary>
        /// 지정된 함수의 인자 정보를 XML 문서 형태로 반환한다.
        /// </summary>
        /// <param name="strFuncName">정보를 획득할 RFC 함수 이름</param>
        /// <param name="strLang">언어</param>
        /// <returns>인자 정보를 갖는 XML 문서</returns>
        public String get_RFCProxyInfo(string strFuncName, string strLang)
        {
            DataTable oDTParam = null;   // RFC 함수 속성(Parameter) 정보 테이블

            XmlDocument oXmlDoc = null;   // XML 문서
            XmlNode oNode = null;         // XML 노드
            String strXML = null;         // RFC 함수 모든 속성(Parameter, Struct) 정보를 나타내는 XML 문서

            try
            {
                oDTParam = RFC_Get_Function_Interface(strFuncName, strLang);
                oDTParam.Namespace = "";         // 네임스페이스를 제거한다.

                // 인자 정보를 XML 문서로 변환한다.
                strXML = ConvertDataTable2XML(oDTParam);
                if (String.IsNullOrEmpty(strXML))
                    return strXML;

                oXmlDoc = new XmlDocument();
                oXmlDoc.PreserveWhitespace = false;
                oXmlDoc.LoadXml(strXML);
                oNode = oXmlDoc.DocumentElement;

                // 하위 노드를 검색하여, 테이블 구조가 있으면 해당 구조를 추가한다.
                GetRFCStructDef(ref oNode);

                strXML = oXmlDoc.InnerXml;
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return strXML;
        }

        /// <summary>
        /// 데이터 테이블로부터 특정 키의 레코드를 반환한다.
        /// </summary>
        /// <param name="oDT">데이터 테이블</param>
        /// <param name="strFieldName">키 항목의 값</param>
        /// <returns>데이터 레코드</returns>
        private DataRow GetDataRowFromDT(DataTable oDT, String strFieldName)
        {
            DataRow[] oDRow = null;
            String strQuery = null;

            try
            {
                strQuery = String.Format("{0} = '{1}'", COLUMN_FIELDNAME, strFieldName);
                oDRow = oDT.Select(strQuery);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oDRow[0];
        }
        /// <summary>
        /// 인터페이스 목록으로부터 테이블, 구조체 형식의 XML 정보를 획득한다.
        /// </summary>
        /// <param name="oParentNode">인터페이스 정보를 갖는 XML 노드</param>
        private void GetRFCStructDef(ref XmlNode oParentNode)
        {
            XmlNodeList oNodeList = null;// XML 노드 리스트
            DataTable oDTStruct = null;  // RFC 함수 속성 중, Struct 형태의 정보 테이블 

            String strParamClass = null;  // RFC 함수 인자 속성 중, PARAMCLASS 항목의 값
            String strExid = null;  // RFC 함수 인자 속성 중, EXID 항목의 값
            String strTabName = null;  // RFC 함수 인자 속성 중, TABNAME 항목의 값

            String strDTXML = null;      // 테이블 XML 문서
            XmlDocument oXmlDoc = null;  // XML 문서
            XmlNode oDTNode = null;      // IMPORT 테이블 노드
            XmlNode oImportedNode = null;// IMPORT 대상 테이블 노드

            X030L oX030LHeader = null;   // X030LHeader Struct
            try
            {
                oXmlDoc = new XmlDocument();
                oXmlDoc.PreserveWhitespace = false;

                // 테이블, 구조를 갖는 하위 노드를 선택한다.
                while (true)
                {
                    // 하위노드 정보가 없는 Table(Struct) 노드를 선택한다.
                    oNodeList = oParentNode.SelectNodes(XPATH_HNODE);
                    if (oNodeList == null || oNodeList.Count == 0)
                        break;

                    for (int i = 0; i < oNodeList.Count; ++i)
                    {
                        // PARAMCLASS 값을 가져온다.
                        strParamClass = strTabName = null;
                        strExid = oNodeList[i].Attributes[COLUMN_EXID].Value;
                        if (String.Equals(oNodeList[i].Name, "RFC_FUNINTTable"))
                        {
                            strParamClass = oNodeList[i].Attributes[COLUMN_PARAMCLASS].Value;
                            // Struct(Table) 구조에 대한 처리를 한다.
                            if (strParamClass.Equals("T")
                                || (!strParamClass.Equals("X") && strExid.Length.Equals(0))
                                || (strExid.Equals("u") || strExid.Equals("h") || strExid.Equals("v"))
                                )
                                strTabName = oNodeList[i].Attributes[COLUMN_TABNAME].Value;
                          }
                        else if (String.Equals(oNodeList[i].Name, "X031LTable"))
                        {
                            // Struct(Table) 구조에 대한 처리를 한다.
                            if (strExid.Equals("u") || strExid.Equals("h") || strExid.Equals("v"))
                                strTabName = oNodeList[i].Attributes[COLUMN_TABTYPE].Value;
                        }

                        if (String.IsNullOrEmpty(strTabName)) continue;

                        // Struct(Table) 구조를 획득한다.
                        oDTStruct = RFC_Get_TabName(strTabName, out oX030LHeader);
                        if (oDTStruct == null || oDTStruct.Rows.Count <= 1)
                        {
                            if (!String.IsNullOrEmpty(oX030LHeader.Refname))
                            {
                                strTabName = oX030LHeader.Refname;
                                oDTStruct = RFC_Get_TabName(strTabName, out oX030LHeader);
                            }
                        }
                        oDTStruct.Namespace = ""; // 네임스페이스를 제거한다.
                        if (oX030LHeader != null) {
                            // 획득한 테이블을 XML 문서로 변환하여, XMLDOM 개체에 로드한다.
                            strDTXML = ConvertHeadere2XML(oX030LHeader);
                            oXmlDoc.LoadXml(strDTXML);
                            oDTNode = oXmlDoc.DocumentElement;

                            // 헤더의 XML 문서를 현재 노드의 자식으로 삼는다. 
                            oImportedNode = oNodeList[i].OwnerDocument.ImportNode(oDTNode, true);
                            oNodeList[i].AppendChild(oImportedNode);
                        }

                        // 획득한 테이블을 XML 문서로 변환하여, XMLDOM 개체에 로드한다.
                        strDTXML = ConvertDataTable2XML(oDTStruct);
                        oXmlDoc.LoadXml(strDTXML);
                        oDTNode = oXmlDoc.DocumentElement;

                        // Table 구조를 획득하고, 구해진 XML 문서를 현재 노드의 자식으로 삼는다. 
                        // 현재 노드의 하위 노드로 설정한다.
                        oImportedNode = oNodeList[i].OwnerDocument.ImportNode(oDTNode, true);
                        oNodeList[i].AppendChild(oImportedNode);
                    } // for
                } // while (true)
            }
            catch (Exception exp)
            {
                throw exp;
            }
            return;
        }

        /// <summary>
        /// X030L 내용을 XML 문서로 변환한다.
        /// </summary>
        /// <param name="oX030L">X030L 개체</param>
        /// <returns>변환된 XML 문서</returns>
        private String ConvertHeadere2XML(X030L oX030L)
        {
            X030LTable oTable = null;

            oTable = new X030LTable();
            oTable.Add(oX030L);

            return ConvertDataTable2XML(oTable.ToADODataTable());
        }
    } // class
}
