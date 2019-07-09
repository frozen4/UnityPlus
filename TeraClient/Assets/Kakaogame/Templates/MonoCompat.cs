#if PLATFORM_KAKAO
using System;

namespace System.CodeDom.Compiler
{
	public class CompileError {
	}
	public class CompilerErrorCollection {
		public void Add(CompilerError e) {
		}
	}
	public class CompilerError {
		public String ErrorText {get;set;}
		public bool IsWarning {get;set;}
	}
}
#endif
