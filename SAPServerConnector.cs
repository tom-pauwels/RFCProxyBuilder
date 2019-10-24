using System;

namespace RFCProxyBuilder
{
	/// <summary>
	/// SAP 서버에 연결한다.
	/// </summary>
	public class SAPServerConnector : SAPProxy
	{
		/// <summary>
		/// SAP 서버 연결 문자열을 생성하는 Helper 클래스 개체
		/// </summary>
		private SAP.Connector.Destination m_oDest = null;

		/// <summary>
		/// 기본 생성자
		/// </summary>
		public SAPServerConnector()
		{
			// SAP 서버 연결 문자열을 생성하는 Helper 클래스 개체를 생성한다.
			m_oDest = new SAP.Connector.Destination();
		}

		/// <summary>
		/// 기본 소멸자
		/// </summary>
		~SAPServerConnector()
		{
			// 연결을 해제한다.
		    this.DisconnectSAPServer();

			// SAP 서버 연결 문자열을 생성하는 Helper 클래스 개체를 소멸한다
			m_oDest.Dispose();
		}

		/// <summary>
		/// 연결 정보를 포함한 생성자
		/// </summary>
		/// <param name="strTYPE">RFC server type, 2/3/E: R/2 or R/3 or External System</param>
		/// <param name="strASHOST">Host name of a specific application server (R/3, No Load Balancing) </param>
		/// <param name="nSYSNR">R/3 system number (R/3, No Load Balancing) </param>
		/// <param name="nCLIENT">SAP logon client</param>
		/// <param name="strLANG">SAP logon language (1-byte SAP language or 2-byte ISO language) </param>
		/// <param name="strUSER">SAP logon user </param>
		/// <param name="strPASSWD">SAP logon password </param>
		public SAPServerConnector(string strTYPE, string strASHOST, short nSYSNR, short nCLIENT, string strLANG, string strUSER, string strPASSWD)
		{
			m_oDest = new SAP.Connector.Destination();

			SetConnectionInfo(strTYPE, strASHOST, nSYSNR, nCLIENT, strLANG, strUSER, strPASSWD);
		}

		/// <summary>
		/// 연결 정보를 설정
		/// </summary>
		/// <param name="strTYPE">RFC server type, 2/3/E: R/2 or R/3 or External System</param>
		/// <param name="strASHOST">Host name of a specific application server (R/3, No Load Balancing) </param>
		/// <param name="nSYSNR">R/3 system number (R/3, No Load Balancing) </param>
		/// <param name="nCLIENT">SAP logon client</param>
		/// <param name="strLANG">SAP logon language (1-byte SAP language or 2-byte ISO language) </param>
		/// <param name="strUSER">SAP logon user </param>
		/// <param name="strPASSWD">SAP logon password </param>
		public void SetConnectionInfo(string strTYPE, string strASHOST, short nSYSNR, short nCLIENT, string strLANG, string strUSER, string strPASSWD)
		{
			m_oDest.Type = strTYPE;
			m_oDest.AppServerHost = strASHOST;
			m_oDest.SystemNumber = nSYSNR;	
			m_oDest.Client = nCLIENT;
			m_oDest.Language = strLANG;
			m_oDest.Username = strUSER;
			m_oDest.Password = strPASSWD;
		}

		/// <summary>
		/// 연결 정보를 설정 (RFC server type 기본값: 3)
		/// </summary>
		/// <param name="strASHOST">Host name of a specific application server (R/3, No Load Balancing) </param>
		/// <param name="nSYSNR">R/3 system number (R/3, No Load Balancing) </param>
		/// <param name="nCLIENT">SAP logon client</param>
		/// <param name="strLANG">SAP logon language (1-byte SAP language or 2-byte ISO language) </param>
		/// <param name="strUSER">SAP logon user </param>
		/// <param name="strPASSWD">SAP logon password </param>
		public void SetConnectionInfo(string strASHOST, short nSYSNR, short nCLIENT, string strLANG, string strUSER, string strPASSWD)
		{
			m_oDest.Type = "3";
			m_oDest.AppServerHost = strASHOST;
			m_oDest.SystemNumber = nSYSNR;	
			m_oDest.Client = nCLIENT;
			m_oDest.Language = strLANG;
			m_oDest.Username = strUSER;
			m_oDest.Password = strPASSWD;
		}

		/// <summary>
		/// SAP 서버에 연결
		/// </summary>
		/// <returns>연결 성공(true), 실패(false) 여부</returns>
		public virtual bool ConnectSAPServer()
		{
			bool bRetVal = false;

			try
			{
				this.ConnectionString = m_oDest.ConnectionString; // 연결 문자열을 설정한다.
				//Return 0 if failed, != 0 else.
				if ( 0 != this.Connection.Open())
					bRetVal = true;
			} 
			catch(Exception exp)
			{
				//Console.WriteLine(exp.Message);
				this.Connection.Close(); // 연결이 실패하는 경우, 강제로 연결을 닫는다.
				throw exp;
			}

			return bRetVal;
		}

		/// <summary>
		/// 연결을 해제한다.
		/// </summary>
		public virtual void DisconnectSAPServer()
		{
            if (this.Connection != null)
            {
                if (this.Connection.IsOpen == true)
                    this.Connection.Close(); // 연결을 해제한다.

            }
        }

	}
}