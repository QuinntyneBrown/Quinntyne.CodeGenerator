﻿using Microsoft.Extensions.DependencyInjection;
using System;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.CodeGenerator.Extensions;
using EventSourcing.CodeGenerator.CLI.Features.EventSourcing;
using EventSourcing.CodeGenerator.CLI.Features.CodeGenerator;

namespace EventSourcing.CodeGenerator.CLI
{
    public class Program
    {
        private readonly Dictionary<string, Func<string[], IRequest>> _commands;

        private readonly ServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            new Program().ProcessArgs(args);
        }

        public Program()
        {
            _serviceProvider = BuildServiceProvider();
            _commands = BuildCommandRequestDictionary();
        }

        public int ProcessArgs(string[] args)
        {
            int lastArg = 0;

            var command = args[lastArg];
            
            var mediator = _serviceProvider.GetService<IMediator>();
            
            var appArgs = (lastArg + 1) >= args.Length ? Enumerable.Empty<string>() : args.Skip(lastArg + 1).ToArray();
            
            Func<string[], IRequest> builtIn;

            if (_commands.TryGetValue(command, out builtIn))
            {
                mediator.Send(builtIn(appArgs.ToArray())).Wait();
            }
   
            return 1;
        }

        private static bool IsArg(string candidate, string longName) => IsArg(candidate, shortName: null, longName: longName);

        private static bool IsArg(string candidate, string shortName, string longName)
        {
            return (shortName != null && candidate.Equals("-" + shortName)) || (longName != null && candidate.Equals("--" + longName));
        }

        static ServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging();

            services.AddMediatR(typeof(Program));

            services.UseEventSourcingCodeGenerator();

            return services.BuildServiceProvider();            
        }


        public Dictionary<string, Func<string[], IRequest>> BuildCommandRequestDictionary()
        {
            var dictionary = new Dictionary<string, Func<string[], IRequest>>();

            RegisterCodeGeneratorCommands.Register(dictionary);
            RegisterEventSourcingCommands.Register(dictionary);

            return dictionary;
        }
    }


}
