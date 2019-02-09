﻿using System.Collections.Generic;
using System.Linq;

using ReactiveUI;

using DotNet.Properties.Pages.Models;
using DotNet.Properties.Services;

namespace DotNet.Properties.Pages.ViewModels
{
    internal class BuildPageViewModel : PropertyPageViewModel
    {
        private static class Property
        {
            // General
            public const string DefineConstants = nameof(DefineConstants);
            public const string PlatformTarget = nameof(PlatformTarget);
            public const string Prefer32Bit = nameof(Prefer32Bit);
            public const string AllowUnsafeBlocks = nameof(AllowUnsafeBlocks);
            public const string Optimize = nameof(Optimize);
            // Errors and warnings
            public const string WarningLevel = nameof(WarningLevel);
            public const string NoWarn = nameof(NoWarn);
            // Output
            public const string OutputPath = nameof(OutputPath);
            public const string AppendTargetFrameworkToOutputPath = nameof(AppendTargetFrameworkToOutputPath);
            public const string AppendRuntimeIdentifierToOutputPath = nameof(AppendRuntimeIdentifierToOutputPath);
            public const string GenerateDocumentationFile = nameof(GenerateDocumentationFile);
            public const string DocumentationFile = nameof(DocumentationFile);
        }

        private static readonly IEnumerable<PlatformTarget> _platformTargets = new List<PlatformTarget>()
        {
            new PlatformTarget("AnyCPU", "Any CPU"),
            new PlatformTarget("x64", "x64"),
            new PlatformTarget("x86", "x86"),
            new PlatformTarget("ARM", "ARM")
        };

        private static readonly IEnumerable<string> _warningLevels = new List<string>()
        {
            "0",
            "1",
            "2",
            "3",
            "4"
        };

        public BuildPageViewModel(IPropertyManager propertyManager)
            : base(propertyManager)
        {
        }

        public string ConditionalCompilationSymbols
        {
            get => GetStringProperty(Property.DefineConstants);
            set => SetStringProperty(Property.DefineConstants, value);
        }

        public IEnumerable<PlatformTarget> AvailablePlatformTargets => _platformTargets;

        public bool PlatformTargetEnabled { get; } = true;

        public PlatformTarget PlatformTarget
        {
            get =>_platformTargets.Single(t => t.Value == (GetStringProperty(Property.PlatformTarget) ?? "AnyCPU"));
            set => SetStringProperty(Property.PlatformTarget, value.Value);
        }

        public bool Prefer32Bit
        {
            get => GetBooleanProperty(Property.Prefer32Bit);
            set => SetBooleanProperty(Property.Prefer32Bit, value);
        }

        public bool AllowUnsafeCode
        {
            get => GetBooleanProperty(Property.AllowUnsafeBlocks);
            set => SetBooleanProperty(Property.AllowUnsafeBlocks, value);
        }

        public bool OptimizeCode
        {
            get => GetBooleanProperty(Property.Optimize);
            set => SetBooleanProperty(Property.Optimize, value);
        }

        public IEnumerable<string> AvailableWarningLevels => _warningLevels;

        public string WarningLevel
        {
            get => GetStringProperty(Property.WarningLevel);
            set => SetStringProperty(Property.WarningLevel, value);
        }

        public string NoWarn
        {
            get => GetStringProperty(Property.NoWarn);
            set => SetStringProperty(Property.NoWarn, value);
        }

        public string OutputPath
        {
            get => GetStringProperty(Property.OutputPath);
            set => SetStringProperty(Property.OutputPath, value);
        }

        public bool AppendTargetFrameworkToOutputPath
        {
            get => GetBooleanProperty(Property.AppendTargetFrameworkToOutputPath);
            set
            {
                SetBooleanProperty(Property.AppendTargetFrameworkToOutputPath, value);
                this.RaisePropertyChanged(nameof(OutputPath));
            }
        }

        public bool AppendRuntimeIdentifierToOutputPath
        {
            get => GetBooleanProperty(Property.AppendRuntimeIdentifierToOutputPath);
            set
            {
                SetBooleanProperty(Property.AppendRuntimeIdentifierToOutputPath, value);
                this.RaisePropertyChanged(nameof(OutputPath));
            }
        }

        public bool GenerateDocumentationFile
        {
            get => GetBooleanProperty(Property.GenerateDocumentationFile);
            set => SetBooleanProperty(Property.GenerateDocumentationFile, value);
        }

        public string DocumentationFile
        {
            get => GetStringProperty(Property.DocumentationFile);
            set => SetStringProperty(Property.DocumentationFile, value);
        }
    }
}
