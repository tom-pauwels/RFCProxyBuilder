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
	/// RFCProxyBuild에 대한 요약 설명입니다.
	/// </summary>
	public class RFCProxyBuild
	{
		/// <summary>
		/// 기본 생성자
		/// </summary>
		public RFCProxyBuild()
		{
		}
		
		/// <summary>
		/// PROXY 소스에서 DLL 파일을 빌드한다.
		/// </summary>
		/// <param name="strProxySource">BUILD 대상 소스 클래스</param>
		/// <param name="strFilePath">출력 파일의 경로</param>
		/// <param name="bClientProxy">Proxy 형태, true:Client, false:Server</param>
		/// <returns>함수 수행 결과 코드</returns>
		protected int BuildRFCProxyDLL(string[] strArrayProxySource, string strFilePath, bool bClientProxy)
		{
			CSharpCodeProvider  oCSProvider = null; // C# 코드 생성기 및 코드 컴파일러의 인스턴스에 액세스
			//ICodeCompiler       iCompiler   = null; // 컴파일러 인터페이스
			CompilerParameters  oParameters = null; // 컴파일러 매개 변수
			TempFileCollection  oTMPCollect = null; // 임시 파일의 컬렉션
			string              strTempDir  = null; // 시스템의 임시 파일 경로

			CompilerResults     oCResults   = null; // 컴파일 결과
			Evidence            evidence    = null; // 보안 정책 관련 설정
			string              strResult   = null; // Compile 결과

			try 
			{
				oCSProvider = new CSharpCodeProvider();     // Provider 개체 생성
				//iCompiler   = oCSProvider.CreateCompiler(); // 컴파일러 인터페이스 생성
				oParameters = new CompilerParameters();     // 컴파일러 매개 변수 개체 생성

				oParameters.GenerateExecutable = false;           // DLL 파일로 생성
				oParameters.GenerateInMemory = false;             // 컴파일러에서 출력을 메모리에 생성해지 않음
				oParameters.TreatWarningsAsErrors = false;        // 경고를 오류로 간주하지 않음
				oParameters.CompilerOptions = "/target:library";  // 라이브러리 형태로 생성
				
				strTempDir = Path.GetTempPath();                  // 현재 시스템의 임시 파일 경로를 반환
				oTMPCollect = new TempFileCollection(strTempDir); // 시스템의 임시 파일 경로 설정
				oCResults = new CompilerResults(oTMPCollect);     // 컴파일 중에 생성되는 파일을 임시 경로에 저장하도록 설정

				evidence = new Evidence(null);
				Url url = new Url("http://rfcschemaproxygenerator");
				evidence.AddHost(url);
				
				oParameters.Evidence = evidence;
				// 참조 어셈블리 정보 설정
				oParameters.ReferencedAssemblies.Add("System.dll");
				oParameters.ReferencedAssemblies.Add("System.Data.dll");
				oParameters.ReferencedAssemblies.Add("System.XML.dll");
				oParameters.ReferencedAssemblies.Add("SAP.Connector.dll");
				oParameters.ReferencedAssemblies.Add("System.Web.Services.dll");
				if (!bClientProxy) // 서버 형태인 경우
					oParameters.ReferencedAssemblies.Add("sap.connector.rfc.dll");

				// 출력 어셈블리 정보 설정
				oParameters.OutputAssembly = strFilePath.ToString(new CultureInfo(Thread.CurrentThread.CurrentUICulture.ToString()));

				// 어셈블리 컴파일
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