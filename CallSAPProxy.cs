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
    /// CallSAPProxy�� ���� ��� �����Դϴ�.
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
        /// �⺻ ������
        /// </summary>
        public CallSAPProxy()
        {
        }

        /// <summary>
        /// �⺻ �Ҹ���
        /// </summary>
        ~CallSAPProxy()
        {
            base.DisconnectSAPServer();
        }

        /// <summary>
        ///  SAP ������ �����Ѵ�.
        /// </summary>
        /// <returns></returns>
        public override bool ConnectSAPServer()
        {
            return base.ConnectSAPServer();
        }

        /// <summary>
        ///  SAP ������ �����Ѵ�.
        /// </summary>
        /// <returns></returns>
        private bool ConnectSAPServer(string strTYPE, string strASHOST, short nSYSNR, short nCLIENT, string strLANG, string strUSER, string strPASSWD)
        {
            bool bRetVal = false;

            base.SetConnectionInfo(strTYPE, strASHOST, nSYSNR, nCLIENT, strLANG, strUSER, strPASSWD); // ȯ�� ������ �����Ѵ�.
            bRetVal = base.ConnectSAPServer(); // SAP ������ �����Ѵ�.

            return bRetVal;
        }

        /// <summary>
        /// SAP �������� ������ �����Ѵ�.
        /// </summary>
        public override void DisconnectSAPServer()
        {
            base.DisconnectSAPServer();
        }

        /// <summary>
        /// RFC �Լ� �̸��� �˻��Ͽ�, �� ����� ��ȯ�Ѵ�. SAP ������ ������� ���� ���, null ���� ��ȯ.
        /// </summary>
        /// <param name="strFuncName">�Լ� �̸� �Ǵ� ����</param>
        /// <param name="strGroupName">�׷� �̸�</param>
        /// <param name="strLang">���</param>
        /// <returns>SAP �Լ� �˻� ��� ���̺� (ADO.NET DataTable)</returns>
        private DataTable RFC_Function_Search(string strFuncName, string strGroupName, string strLang)
        {
            RFCFUNCTable oRfcTbl = null;

            // SAP ������ ������� ���� ���, null ���� ��ȯ�ϰ� �Լ��� �����Ѵ�.
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
        /// �Լ��� ���� ������ ��ȯ�Ѵ�. SAP ������ ������� ���� ���, null ���� ��ȯ.
        /// </summary>
        /// <param name="strFuncName">�Լ� �̸�</param>
        /// <param name="strLang">���</param>
        /// <returns>SAP �Լ��� ���� ���� (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Function_Interface(string strFuncName, string strLang)
        {
            RFC_FUNINTTable oRFCFT = null; // RFC_FUNINT Ŭ����

            // SAP ������ ������� ���� ���, null ���� ��ȯ�ϰ� �Լ��� �����Ѵ�.
            if (this.Connection.IsOpen == false)
                return null;

            try
            {
                oRFCFT = new RFC_FUNINTTable();

                // none_unicode_length => ����� 'X'�� ���
                // ������ ��쿣 unicode ���̸�, 'X'�� ��쿣 non-unicode �Ķ���� ���̸� ��ȯ��.
                RFC_GET_FUNCTION_INTERFACE(strFuncName, strLang, "X", ref oRFCFT);
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oRFCFT.ToADODataTable(); // Converts the SAP table to an ADO.NET DataTable.  
        }

        /// <summary>
        /// RFC ����ü(���̺�) ������ ���� ������ ��ȯ�Ѵ�. SAP ������ ������� ���� ���, null ���� ��ȯ
        /// </summary>
        /// <param name="strTabName">����ü(���̺�) �̸�</param>
        /// <param name="nTabLen">����ü(���̺�) ����</param>
        /// <returns>RFC ����ü(���̺�) ������ ���� ���� (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Structure_Definition(string strTabName, out int nTabLen)
        {
            RFC_FIELDSTable oRFCFT = null;
            nTabLen = -1;

            // SAP ������ ������� ���� ���, null ���� ��ȯ�ϰ� �Լ��� �����Ѵ�.
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
        /// �����ڵ� ������ RFC ����ü(���̺�) ������ ���� ������ ��ȯ�Ѵ�. SAP ������ ������� ���� ���, null ���� ��ȯ
        /// </summary>
        /// <param name="strTabName">����ü(���̺�) �̸�</param>
        /// <param name="nLen1">����ü(���̺�) ����1</param>
        /// <param name="nLen2">����ü(���̺�) ����2</param>
        /// <returns>RFC ����ü(���̺�) ������ ���� ���� (ADO.NET DataTable)</returns>
        private DataTable RFC_Get_Unicode_Structure(string strTabName, out int nLen1, out int nLen2)
        {
            int nB1TabLen = 0;
            int nB2TabLen = 0;
            int nB4TabLen = 0;
            int nCharLen = 0;
            byte[] btUUID = null;

            RFC_FLDS_UTable oFields = null;

            nLen1 = nLen2 = -1;

            // SAP ������ ������� ���� ���, null ���� ��ȯ�ϰ� �Լ��� �����Ѵ�.
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
        /// ���̺� ����
        /// </summary>
        /// <param name="strTabName">���̺� �̸�</param>
        /// <param name="oHeader"> ADO.NET DataTable of Database Structure DDNTT</param>
        /// <returns>RFC ����ü(���̺�) ������ ���� ���� (ADO.NET DataTable of X031L elements)</returns>
        private DataTable RFC_Get_TabName(String strTabName, out X030L oHeader)
        {
			X031LTable oNameTab  = null;
            oHeader = null;

            // SAP ������ ������� ���� ���, null ���� ��ȯ�ϰ� �Լ��� �����Ѵ�.
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
        /// DataTable ��ü���� XML ������ �����Ͽ� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="oDT">DataTable ��ü</param>
        /// <returns>XML ����</returns>
        private String ConvertDataTable2XML(DataTable oDT)
        {
            String strXML = null;       // ��ȯ�� XML ����
            DataSet oDS = null;         // ���̺��� XML ������ �����ϱ� ���� �ʿ�
            String strNamespace = null; // ������ ���ӽ����̽�
            int nColIdx = 0; // Column Index
            int nRowIdx = 0; // Row Index

            DataTable oDTClone = null; // Clone DataTable 
            DataRow oRow = null; // // Clone Record
            try
            {
                // Attribute ������ XML ������ �����ϱ� ���Ͽ�, Table �Ӽ��� �����Ѵ�.
                for (nColIdx = 0; nColIdx < oDT.Columns.Count; ++nColIdx)
                    oDT.Columns[nColIdx].ColumnMapping = MappingType.Attribute;

                // "X030LTable", "X031LTable" ���̺� ���� �۾��� �����Ѵ�.
                if (String.Equals(oDT.TableName, "X031LTable") || String.Equals(oDT.TableName, "X030LTable"))
                {
                    // Clone Table ����
                    oDTClone = oDT.Clone();
                    // Clone Table ���� ������ �����Ѵ�.
                    for (nColIdx = 0; nColIdx < oDTClone.Columns.Count; ++nColIdx)
                    {
                        // Byte[] ������ ���� ������ �����Ѵ�.
                        if (oDTClone.Columns[nColIdx].DataType.Equals(Type.GetType("System.Byte[]")))
                            oDTClone.Columns[nColIdx].DataType = Type.GetType("System.Int32");
                    } // for

                    // ���� Table �����͸� ��ȯ�Ͽ� Clone ���̺�� �����Ѵ�.
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

                oDS = new DataSet(); // DataSet ��ü�� ����
                if (oDTClone != null)
                    oDS.Tables.Add(oDTClone); // ���̺��� �߰�.
                else
                    oDS.Tables.Add(oDT); // ���̺��� �߰�.
                // ���̺� �̸����� ROOT ����� �̸��� �����Ѵ�.
                oDS.DataSetName = "ROOT_" + oDT.TableName;

                //Load the document with the DataSet.
                XmlDataDocument oXMLDOC = new XmlDataDocument(oDS);

                // XML ������ ȹ���Ѵ�.
                strXML = oXMLDOC.InnerXml;

                // ���� ���α׷��� ���ӽ����̽��� ȹ���Ѵ�.
                strNamespace = String.Format("xmlns=\"{0}\"", this.GetType().Namespace);

                // XML �������� ���ӽ����̽��� ���ʿ��� ������ �����Ѵ�.
                strXML = strXML.Replace(strNamespace, "");
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return strXML;
        }

        /// <summary>
        /// 256 ������ ����Ʈ �迭�� ���� ��ȯ�� �����Ѵ�.
        /// </summary>
        /// <param name="bt256">256 ������ ����Ʈ �迭</param>
        /// <returns>��ȯ�� 10���� ����</returns>
        private int ConvertByte2Int(Byte[] bt256)
        {
            Double dblRetVal = 0;
            int nSize = bt256.Length - 1; // 256 ������ ����Ʈ �迭 �ε���

            for (int i = nSize; i >= 0; --i)
            {
                dblRetVal += ((Double)bt256[i] * Math.Pow(0x100, nSize - i));
            }

            return (int)dblRetVal;
        }

        /// <summary>
        /// RFC �Լ� �̸��� �˻��Ͽ�, �� ����� ��ȯ�Ѵ�. SAP ������ ������� ���� ���, null ���� ��ȯ.
        /// </summary>
        /// <param name="strFilter">�Լ� �̸� �Ǵ� ����</param>
        /// <param name="strGroupName">�׷� �̸�</param>
        /// <param name="strLang">���</param>
        /// <returns>SAP �Լ� �˻� ��� ���̺� (ADO.NET DataTable)</returns>
        public DataTable get_RFCFunctionNames(string strFilter, string strGroupName, string strLang)
        {
            DataTable oDT = null;   // ������ ���̺� ��ü			
            try
            {
                oDT = RFC_Function_Search(strFilter, strGroupName, strLang); // RFC �Լ� ȣ��
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return oDT;
        }

        /// <summary>
        /// ������ �Լ��� ���� ������ XML ���� ���·� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="strFuncName">������ ȹ���� RFC �Լ� �̸�</param>
        /// <param name="strLang">���</param>
        /// <returns>���� ������ ���� XML ����</returns>
        public String get_RFCProxyInfo(string strFuncName, string strLang)
        {
            DataTable oDTParam = null;   // RFC �Լ� �Ӽ�(Parameter) ���� ���̺�

            XmlDocument oXmlDoc = null;   // XML ����
            XmlNode oNode = null;         // XML ���
            String strXML = null;         // RFC �Լ� ��� �Ӽ�(Parameter, Struct) ������ ��Ÿ���� XML ����

            try
            {
                oDTParam = RFC_Get_Function_Interface(strFuncName, strLang);
                oDTParam.Namespace = "";         // ���ӽ����̽��� �����Ѵ�.

                // ���� ������ XML ������ ��ȯ�Ѵ�.
                strXML = ConvertDataTable2XML(oDTParam);
                if (String.IsNullOrEmpty(strXML))
                    return strXML;

                oXmlDoc = new XmlDocument();
                oXmlDoc.PreserveWhitespace = false;
                oXmlDoc.LoadXml(strXML);
                oNode = oXmlDoc.DocumentElement;

                // ���� ��带 �˻��Ͽ�, ���̺� ������ ������ �ش� ������ �߰��Ѵ�.
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
        /// ������ ���̺�κ��� Ư�� Ű�� ���ڵ带 ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="oDT">������ ���̺�</param>
        /// <param name="strFieldName">Ű �׸��� ��</param>
        /// <returns>������ ���ڵ�</returns>
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
        /// �������̽� ������κ��� ���̺�, ����ü ������ XML ������ ȹ���Ѵ�.
        /// </summary>
        /// <param name="oParentNode">�������̽� ������ ���� XML ���</param>
        private void GetRFCStructDef(ref XmlNode oParentNode)
        {
            XmlNodeList oNodeList = null;// XML ��� ����Ʈ
            DataTable oDTStruct = null;  // RFC �Լ� �Ӽ� ��, Struct ������ ���� ���̺� 

            String strParamClass = null;  // RFC �Լ� ���� �Ӽ� ��, PARAMCLASS �׸��� ��
            String strExid = null;  // RFC �Լ� ���� �Ӽ� ��, EXID �׸��� ��
            String strTabName = null;  // RFC �Լ� ���� �Ӽ� ��, TABNAME �׸��� ��

            String strDTXML = null;      // ���̺� XML ����
            XmlDocument oXmlDoc = null;  // XML ����
            XmlNode oDTNode = null;      // IMPORT ���̺� ���
            XmlNode oImportedNode = null;// IMPORT ��� ���̺� ���

            X030L oX030LHeader = null;   // X030LHeader Struct
            try
            {
                oXmlDoc = new XmlDocument();
                oXmlDoc.PreserveWhitespace = false;

                // ���̺�, ������ ���� ���� ��带 �����Ѵ�.
                while (true)
                {
                    // ������� ������ ���� Table(Struct) ��带 �����Ѵ�.
                    oNodeList = oParentNode.SelectNodes(XPATH_HNODE);
                    if (oNodeList == null || oNodeList.Count == 0)
                        break;

                    for (int i = 0; i < oNodeList.Count; ++i)
                    {
                        // PARAMCLASS ���� �����´�.
                        strParamClass = strTabName = null;
                        strExid = oNodeList[i].Attributes[COLUMN_EXID].Value;
                        if (String.Equals(oNodeList[i].Name, "RFC_FUNINTTable"))
                        {
                            strParamClass = oNodeList[i].Attributes[COLUMN_PARAMCLASS].Value;
                            // Struct(Table) ������ ���� ó���� �Ѵ�.
                            if (strParamClass.Equals("T")
                                || (!strParamClass.Equals("X") && strExid.Length.Equals(0))
                                || (strExid.Equals("u") || strExid.Equals("h") || strExid.Equals("v"))
                                )
                                strTabName = oNodeList[i].Attributes[COLUMN_TABNAME].Value;
                          }
                        else if (String.Equals(oNodeList[i].Name, "X031LTable"))
                        {
                            // Struct(Table) ������ ���� ó���� �Ѵ�.
                            if (strExid.Equals("u") || strExid.Equals("h") || strExid.Equals("v"))
                                strTabName = oNodeList[i].Attributes[COLUMN_TABTYPE].Value;
                        }

                        if (String.IsNullOrEmpty(strTabName)) continue;

                        // Struct(Table) ������ ȹ���Ѵ�.
                        oDTStruct = RFC_Get_TabName(strTabName, out oX030LHeader);
                        if (oDTStruct == null || oDTStruct.Rows.Count <= 1)
                        {
                            if (!String.IsNullOrEmpty(oX030LHeader.Refname))
                            {
                                strTabName = oX030LHeader.Refname;
                                oDTStruct = RFC_Get_TabName(strTabName, out oX030LHeader);
                            }
                        }
                        oDTStruct.Namespace = ""; // ���ӽ����̽��� �����Ѵ�.
                        if (oX030LHeader != null) {
                            // ȹ���� ���̺��� XML ������ ��ȯ�Ͽ�, XMLDOM ��ü�� �ε��Ѵ�.
                            strDTXML = ConvertHeadere2XML(oX030LHeader);
                            oXmlDoc.LoadXml(strDTXML);
                            oDTNode = oXmlDoc.DocumentElement;

                            // ����� XML ������ ���� ����� �ڽ����� ��´�. 
                            oImportedNode = oNodeList[i].OwnerDocument.ImportNode(oDTNode, true);
                            oNodeList[i].AppendChild(oImportedNode);
                        }

                        // ȹ���� ���̺��� XML ������ ��ȯ�Ͽ�, XMLDOM ��ü�� �ε��Ѵ�.
                        strDTXML = ConvertDataTable2XML(oDTStruct);
                        oXmlDoc.LoadXml(strDTXML);
                        oDTNode = oXmlDoc.DocumentElement;

                        // Table ������ ȹ���ϰ�, ������ XML ������ ���� ����� �ڽ����� ��´�. 
                        // ���� ����� ���� ���� �����Ѵ�.
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
        /// X030L ������ XML ������ ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="oX030L">X030L ��ü</param>
        /// <returns>��ȯ�� XML ����</returns>
        private String ConvertHeadere2XML(X030L oX030L)
        {
            X030LTable oTable = null;

            oTable = new X030LTable();
            oTable.Add(oX030L);

            return ConvertDataTable2XML(oTable.ToADODataTable());
        }
    } // class
}
