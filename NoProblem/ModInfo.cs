using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using NoProblem.Redirection;
using UnityEngine;

namespace NoProblem
{
    public class ModInfo : LoadingExtensionBase, IUserMod
    {
        public string Name => "No Problem Notifications";

        public string Description => "Prevents problem notification flags from being set.";

        internal static string m_name = "No Problem Notifications";

        private static Options m_options;

        private UICheckBox removeExistingProblems;

        private UICheckBox[] checkBoxes = new UICheckBox[50];

        internal static Dictionary<Problem, Notification.Problem1> ProblemTranslator;

        internal static Options Options
        {
            get
            {
                if (ModInfo.m_options == null)
                {
                    ModInfo.m_options = Options.Load();
                    if (ModInfo.m_options == null)
                    {
                        ModInfo.m_options = new Options();
                    }
                    ModInfo.m_options.Save();
                }
                return ModInfo.m_options;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (this.checkBoxes.Length > ModInfo.Options.ProblemOptions.Length)
            {
                Array.Resize<bool>(ref ModInfo.Options.ProblemOptions, this.checkBoxes.Length);
            }
            this.removeExistingProblems = (UICheckBox)helper.AddCheckbox("REMOVE PRE-EXISTING NOTIFICATION FLAGS", ModInfo.Options.RemoveExistingProblems, delegate (bool b)
            {
                ModInfo.Options.RemoveExistingProblems = b;
                ModInfo.Options.Save();
                if (b)
                {
                    this.RemoveAllExistingProblems();
                }
            });
            this.SetCheckboxProperties(this.removeExistingProblems);
            this.removeExistingProblems.tooltip = "Enable this checkbox to remove all existing notification flags that you have disabled below.";
            for (int i = 0; i < 50; i++)
            {
                Problem problem = (Problem)i;
                this.checkBoxes[i] = (UICheckBox)helper.AddCheckbox("Disable " + problem.ToString() + " Notification", ModInfo.Options.ProblemOptions[i], delegate (bool b)
                {
                    this.OnCheckBoxToggled(problem, b);
                });
                this.SetCheckboxProperties(this.checkBoxes[i]);
            }
        }

        private void SetCheckboxProperties(UICheckBox checkbox)
        {
            checkbox.Find<UILabel>("Label").textScale = 0.8f;
            checkbox.Find<UISprite>("Checked").size = new Vector2(12f, 12f);
            checkbox.Find<UISprite>("Unchecked").size = new Vector2(12f, 12f);
            checkbox.height = 13f;
        }

        private void OnCheckBoxToggled(Problem problem, bool state)
        {
            ModInfo.Options.ProblemOptions[(int)problem] = state;
            ModInfo.Options.Save();
            if (ModInfo.Options.RemoveExistingProblems && state)
            {
                this.RemoveExistingProblems(problem);
            }
        }

        private void RemoveExistingProblems(Problem problem)
        {
            Singleton<SimulationManager>.instance.AddAction(delegate ()
            {
                Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                ushort num = 0;
                while (num < buffer.Length)
                {
                    if (buffer[num].m_flags != Building.Flags.None)
                    {
                        Notification.Problem1 problems = buffer[num].m_problems;
                        if (problems != Notification.Problem1.None)
                        {
                            Notification.Problem1 problem2 = Notification.RemoveProblems(problems, NotificationDetour.ConvertToNotificationProblem(problem));
                            buffer[num].m_problems = problem2;
                            Singleton<BuildingManager>.instance.UpdateNotifications(num, problems, problem2);
                        }
                    }
                    num += 1;
                }
                NetNode[] buffer2 = Singleton<NetManager>.instance.m_nodes.m_buffer;
                ushort num2 = 0;
                while (num2 < buffer2.Length)
                {
                    if (buffer2[num2].m_flags != NetNode.Flags.None)
                    {
                        Notification.Problem1 problems2 = buffer2[num2].m_problems;
                        if (problems2 != Notification.Problem1.None)
                        {
                            Notification.Problem1 problem3 = Notification.RemoveProblems(problems2, NotificationDetour.ConvertToNotificationProblem(problem));
                            buffer2[num2].m_problems = problem3;
                            Singleton<NetManager>.instance.UpdateNodeNotifications(num2, problems2, problem3);
                        }
                    }
                    num2 += 1;
                }
            });
        }

        private void RemoveAllExistingProblems()
        {
            for (int i = 0; i < 50; i++)
            {
                if (ModInfo.Options.ProblemOptions[i])
                {
                    this.RemoveExistingProblems((Problem)i);
                }
            }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            ModInfo.ProblemTranslator = new Dictionary<Problem, Notification.Problem1>();
            Enum.GetNames(typeof(Notification.Problem1));
            Notification.Problem1[] array = (Notification.Problem1[])Enum.GetValues(typeof(Notification.Problem1));
            for (int i = 0; i < 50; i++)
            {
                ModInfo.ProblemTranslator.Add((Problem)i, array[i + 1]);
            }
            Redirector<NotificationDetour>.Deploy();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Redirector<NotificationDetour>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (ModInfo.Options.RemoveExistingProblems)
            {
                this.RemoveAllExistingProblems();
            }
        }
    }
}
