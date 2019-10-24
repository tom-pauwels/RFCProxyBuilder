using System;
using System.IO;
using System.Threading;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Policy;
using System.Globalization;
using Microsoft.CSharp;

namespace RFCProxyBuilder
{
	/// <summary>
	/// RFCProxyBuild�� ���� ��� �����Դϴ�.
	/// </summary>
	public class RFCProxyBuild
	{
		/// <summary>
		/// �⺻ ������
		/// </summary>
		public RFCProxyBuild()
		{
		}
		
		/// <summary>
		/// PROXY �ҽ����� DLL ������ �����Ѵ�.
		/// </summary>
		/// <param name="strProxySource">BUILD ��� �ҽ� Ŭ����</param>
		/// <param name="strFilePath">��� ������ ���</param>
		/// <param name="bClientProxy">Proxy ����, true:Client, false:Server</param>
		/// <returns>�Լ� ���� ��� �ڵ�</returns>
		protected int BuildRFCProxyDLL(string[] strArrayProxySource, string strFilePath, bool bClientProxy)
		{
			CSharpCodeProvider  oCSProvider = null; // C# �ڵ� ������ �� �ڵ� �����Ϸ��� �ν��Ͻ��� �׼���
			//ICodeCompiler       iCompiler   = null; // �����Ϸ� �������̽�
			CompilerParameters  oParameters = null; // �����Ϸ� �Ű� ����
			TempFileCollection  oTMPCollect = null; // �ӽ� ������ �÷���
			string              strTempDir  = null; // �ý����� �ӽ� ���� ���

			CompilerResults     oCResults   = null; // ������ ���
			Evidence            evidence    = null; // ���� ��å ���� ����
			string              strResult   = null; // Compile ���

			try 
			{
				oCSProvider = new CSharpCodeProvider();     // Provider ��ü ����
				//iCompiler   = oCSProvider.CreateCompiler(); // �����Ϸ� �������̽� ����
				oParameters = new CompilerParameters();     // �����Ϸ� �Ű� ���� ��ü ����

				oParameters.GenerateExecutable = false;           // DLL ���Ϸ� ����
				oParameters.GenerateInMemory = false;             // �����Ϸ����� ����� �޸𸮿� �������� ����
				oParameters.TreatWarningsAsErrors = false;        // ����� ������ �������� ����
				oParameters.CompilerOptions = "/target:library";  // ���̺귯�� ���·� ����
				
				strTempDir = Path.GetTempPath();                  // ���� �ý����� �ӽ� ���� ��θ� ��ȯ
				oTMPCollect = new TempFileCollection(strTempDir); // �ý����� �ӽ� ���� ��� ����
				oCResults = new CompilerResults(oTMPCollect);     // ������ �߿� �����Ǵ� ������ �ӽ� ��ο� �����ϵ��� ����

				evidence = new Evidence(null);
				Url url = new Url("http://rfcschemaproxygenerator");
				evidence.AddHost(url);
				
				oParameters.Evidence = evidence;
				// ���� ������� ���� ����
				oParameters.ReferencedAssemblies.Add("System.dll");
				oParameters.ReferencedAssemblies.Add("System.Data.dll");
				oParameters.ReferencedAssemblies.Add("System.XML.dll");
				oParameters.ReferencedAssemblies.Add("SAP.Connector.dll");
				oParameters.ReferencedAssemblies.Add("System.Web.Services.dll");
				if (!bClientProxy) // ���� ������ ���
					oParameters.ReferencedAssemblies.Add("sap.connector.rfc.dll");

				// ��� ������� ���� ����
				oParameters.OutputAssembly = strFilePath.ToString(new CultureInfo(Thread.CurrentThread.CurrentUICulture.ToString()));

				// ������� ������
				//oCResults = iCompiler.CompileAssemblyFromSourceBatch(oParameters, strArrayProxySource);
                oCResults = oCSProvider.CompileAssemblyFromSource(oParameters, strArrayProxySource);
				oCResults.Evidence.AddHost(url);

				if (!oCResults.Errors.Count.Equals(0))
				{
					for (int idx = 0; idx < oCResults.Errors.Count; ++idx)
					{
						strResult = strResult + oCResults.Errors[idx].ToString() + " ";
					}
					throw new Exception(strResult);
				}
			} 
			catch (Exception exp)
			{
				throw exp;
			}
			finally 
			{
				oTMPCollect.Delete();
			}
			return oCResults.Errors.Count;
		}
	} // class
}