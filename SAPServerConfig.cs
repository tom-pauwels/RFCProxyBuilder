using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;

using System.Collections;
using System.ComponentModel;

namespace RFCProxyBuilder
{
	/// <summary>
	/// SAPServerConfig에 대한 요약 설명입니다.
	/// </summary>
	public class SAPServerConfig
	{
		#region Internal Class
		/// <summary>
		/// PropertyGrid 설정(표시)을 위한 클래스
		/// </summary>
		internal class SAPSeverInfo 
		{
			public SAPServerConfig owner;

			public SAPSeverInfo(SAPServerConfig owner) 
			{
				this.owner = owner;
			}

			[Category("SAP Sever 등록 내역(이름)")]
			[Description("SAP Sever 사용자 등록 내역(이름)")]
			public string NAME
			{
				get 
				{
					return owner.m_strName;
				}
				set 
				{
					owner.m_strName = value;
				}
			}

			[Category("SAP Server 등록 정보")]
			[Description("Host name or IP address of a specific application server (R/3, No Load Balancing)")]
			public string ASHOST
			{
				get 
				{
					return owner.m_strASHOST;
				}
				set 
				{
					owner.m_strASHOST = value;
				}
			}

			[Category("SAP Server 등록 정보")]
			[Description("R/3 system number (R/3, No Load Balancing)")]
			public string SYSNR 
			{
				get 
				{
					return owner.m_strSYSNR;
				}
				set 
				{
					owner.m_strSYSNR = value;
				}
			}

			[Category("SAP Server 등록 정보")]
			[Description("SAP logon client number")]
			public string CLIENT 
			{
				get 
				{
					return owner.m_strCLIENT;
				}
				set 
				{
					owner.m_strCLIENT = value;
				}
			}

			[Category("SAP Server 등록 정보")]
			[Description("SAP logon language (1-byte SAP language or 2-byte ISO language)")]
			public string LANG 
			{
				get 
				{
					return owner.m_strLANG;
				}
				set 
				{
					owner.m_strLANG = value;
				}
			}

		} // class

		#endregion

		private const String XML_FILE_NAME = "SAPINFO.XML";

		private string m_strName   = "";
		private string m_strASHOST = "";
		private string m_strSYSNR    = "";
		private string m_strCLIENT   = "";
		private string m_strLANG   = "EN";

		public SAPServerConfig()
		{
		}

		/// <summary>
		/// SAP Server 구성 정보를 읽는다.
		/// </summary>
		/// <param name="strXMLFileName">환경 구성 XML 파일</param>
		/// <returns>XML 파일에서 읽은 SAP 서버 목록</returns>
		public string LoadConfigFile(string strXMLFileName)
		{
			XmlDocument oXMLDoc     = null;
			string         strXML       = null;
		
			try 
			{
				oXMLDoc = new XmlDocument ();
				oXMLDoc.PreserveWhitespace = false;

				oXMLDoc.Load(strXMLFileName);
				strXML = oXMLDoc.InnerXml;

			}
			catch (System.IO.FileNotFoundException)
			{
				strXML = null;
			}
			catch (Exception exp)
			{
				throw exp;
			}

			return strXML;
		}

		/// <summary>
		/// SAP Server 구성 정보를 저장한다.
		/// </summary>
		/// <param name="strXMLFileName">환경 구성 XML 파일</param>
		/// <param name="strXML">환경 구성 XML 문서</param>
		/// <returns>저장 성공 여부</returns>
		public bool SaveConfigFile(string strXMLFileName, string strXML)
		{
			XmlDocument oXMLDoc = null;
			bool        bResult = false;
			
			try
			{
				if (String.IsNullOrEmpty(strXML))
					return bResult;

				oXMLDoc = new XmlDocument();
				oXMLDoc.PreserveWhitespace = false;

				oXMLDoc.LoadXml(strXML);

				oXMLDoc.Save(strXMLFileName);

				bResult = true;
			}
			catch (System.IO.FileNotFoundException)
			{
				bResult = false;
			}
			catch (Exception exp)
			{
				throw exp;
			}

			return bResult;
		}

		/// <summary>
		/// 환경 구성 파일에 서버 등록 정보를 삽입한다.
		/// </summary>
		/// <param name="strXML">원본 XML 문서</param>
		/// <returns>함수의 성공 여부</returns>
		public bool AddConfiguration(ref string strXML)
		{
			XmlDocument     oXMLDoc     = null;
			XmlElement      oRootNode   = null;
			XmlElement      oSubNode    = null;
			XmlAttribute    oAttr       = null;
			XmlNodeList     oNodeList   = null;
			bool            bRetVal     = false;

			try 
			{
				oXMLDoc = new XmlDocument ();
				oXMLDoc.PreserveWhitespace = false;

				if (String.IsNullOrEmpty(strXML)) 
				{
					oRootNode = (XmlElement)oXMLDoc.CreateNode(XmlNodeType.Element, "", "SAPServer", "");
				}
				else 
				{
					oXMLDoc.LoadXml(strXML);
					oNodeList = SearchConfiguration(strXML, this.m_strName);
					if (oNodeList.Count >= 1)
						return bRetVal;

					oRootNode = (XmlElement)oXMLDoc.FirstChild;
				}

				oSubNode  = (XmlElement)oXMLDoc.CreateNode(XmlNodeType.Element, "", "ServerInfo", "");

				oAttr = oXMLDoc.CreateAttribute("NAME");
				oAttr.Value = m_strName;
				oSubNode.SetAttributeNode(oAttr);

				oAttr = oXMLDoc.CreateAttribute("ASHOST");
				oAttr.Value = m_strASHOST;
				oSubNode.SetAttributeNode(oAttr);

				oAttr = oXMLDoc.CreateAttribute("SYSNR");
				oAttr.Value = m_strSYSNR;
				oSubNode.SetAttributeNode(oAttr);

				oAttr = oXMLDoc.CreateAttribute("CLIENT");
				oAttr.Value = m_strCLIENT;
				oSubNode.SetAttributeNode(oAttr);

				oAttr = oXMLDoc.CreateAttribute("LANG");
				oAttr.Value = m_strLANG;
				oSubNode.SetAttributeNode(oAttr);

				oRootNode.AppendChild(oSubNode);
				oXMLDoc.AppendChild(oRootNode);

				strXML = oXMLDoc.InnerXml;

				bRetVal = true;
			}
			catch (System.IO.FileNotFoundException)
			{
				bRetVal = false;
			}
			catch (Exception exp)
			{
				throw exp;
			}

			return bRetVal;
		}


		/// <summary>
		/// XML 문서에서 지정된 SAP 내역을 제거한다.
		/// </summary>
		/// <param name="strXML">XML 문서</param>
		/// <returns>함수의 성공 여부</returns>
		public bool RemoveConfiguration(ref string strXML)
		{
			XmlDocument     oXMLDoc     = null;
			XmlNode         oNode       = null;
			string          strXPath    = null;
			bool            bRetVal     = false;

			try 
			{
				oXMLDoc = new XmlDocument ();
				oXMLDoc.PreserveWhitespace = false;

				oXMLDoc.LoadXml(strXML);
				strXPath = String.Format("//*[@NAME='{0}']", this.m_strName);
				oNode = oXMLDoc.SelectSingleNode(strXPath);
				if (oNode != null)
				{
					oXMLDoc.DocumentElement.RemoveChild(oNode);
					strXML = oXMLDoc.InnerXml;
					bRetVal = true;
				}
			} 
			catch (Exception exp)
			{
				throw exp;
			}
			return bRetVal;
		}

		/// <summary>
		/// 환경 구성 문서에서 이름을 검색한다.
		/// </summary>
		/// <param name="strXMLDOC">XML 문서</param>
		/// <param name="strSAPName">SAP 이름</param>
		/// <returns>찾은 노드 리스트</returns>
		public XmlNodeList SearchConfiguration(string strXMLDOC, string strSAPName)
		{
			XmlNodeList  oNodeList = null; // 테이블 형식의 노드 리스트
			XmlDocument  oXMLDOC   = null;
			string       strXPath  = null;
			try
			{
				oXMLDOC  = new XmlDocument();
				oXMLDOC.PreserveWhitespace = false;
				oXMLDOC.LoadXml(strXMLDOC);

				strXPath = String.Format("//*[@NAME = '{0}']", strSAPName);

				oNodeList  = oXMLDOC.SelectNodes(strXPath); // 노드 리스트를 가져온다.
			}
			catch (Exception exp)
			{
				throw exp;
			}

			return oNodeList;
		}

		/// <summary>
		/// SAP 서버 목록을 구한다.
		/// </summary>
		/// <param name="strXMLDOC">XML 문서</param>
		/// <returns>SAP 서버 목록</returns>
		public String[] GetServerNames(string strXMLDOC)
		{
			string[] arrNames = null;

			XmlNodeList  oNodeList = null; // 테이블 형식의 노드 리스트
			XmlDocument  oXMLDOC   = null;

			if (strXMLDOC == null) 
				return arrNames;

			try
			{
				oXMLDOC = new XmlDocument();
				oXMLDOC.PreserveWhitespace = false;
				oXMLDOC.LoadXml(strXMLDOC);

				oNodeList = oXMLDOC.SelectNodes("//@NAME");
			
				arrNames = new string[oNodeList.Count];
				for (int i = 0; i < oNodeList.Count; ++i)
				{
					arrNames[i] = oNodeList[i].Value;
				}
			}
			catch (Exception exp)
			{
				throw exp;
			}
			return arrNames;
		}
	} // class
}
