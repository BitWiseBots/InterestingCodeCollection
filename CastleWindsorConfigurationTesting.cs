	//These methods are meant to be used in a unit test to fail on bad Windsor configurations
	 public static class IocUtil
    {	
	//This method was found on StackOverflow at http://stackoverflow.com/questions/8969973/castle-windsor-is-there-a-way-of-validating-registration-without-a-resolve-call
	//It was slightly modified to make resharper happy
		public static void AssertNoMisconfiguredComponents(IWindsorContainer container)
        {
            var host = (IDiagnosticsHost)container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
            var diagnostics = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();

            var handlers = diagnostics.Inspect();

            if (!handlers.Any()) return;

            var message = new StringBuilder();
            var inspector = new DependencyInspector(message);

            foreach (var info in handlers.OfType<IExposeDependencyInfo>())
            {
                info.ObtainDependencyDetails(inspector);
            }

            Assert.Fail(message.ToString());
        }
		
		//I based this method on the previously mention SO question, on the documentation found here https://github.com/castleproject/Windsor/blob/master/docs/debugger-views.md
		//and also on some debug time inspection that I did as the handlers for this Diagnostic type are vastly different than the original
		//as such this is possibly not the best implementation
        public static void AssertNoLifestyleMismatchedComponents(IWindsorContainer container)
        {
            var host = (IDiagnosticsHost)container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
            var diagnostics = host.GetDiagnostic<IPotentialLifestyleMismatchesDiagnostic>();

            var handlers = diagnostics.Inspect();

            if (!handlers.Any()) return;

            var message = new StringBuilder();

            foreach (var handlerSet in handlers)
            {
                message.AppendFormat(
                    "Detected a lifestyle mismatch between '{0}' with '{1}' and '{2}' with '{3}'\n",
                    handlerSet[0].ComponentModel.ComponentName, handlerSet[0].ComponentModel.LifestyleType,
                    handlerSet[1].ComponentModel.ComponentName, handlerSet[1].ComponentModel.LifestyleType);
            }

            Assert.Fail(message.ToString());
        }
	}