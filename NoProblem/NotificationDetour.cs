using System;
using NoProblem.Redirection;

namespace NoProblem
{
    [TargetType(typeof(Notification))]
    public class NotificationDetour
    {
        internal static Notification.Problem1 ConvertToNotificationProblem(Problem problem)
        {
            string[] names = Enum.GetNames(typeof(Notification.Problem1));
            Notification.Problem1[] array = (Notification.Problem1[])Enum.GetValues(typeof(Notification.Problem1));
            for (int i = 0; i < 50; i++)
            {
                if (names[i] == problem.ToString())
                {
                    return array[i];
                }
            }
            return 0L;
        }

        private static Problem ConvertToModProblem(Notification.Problem1 problem)
        {
            string[] names = Enum.GetNames(typeof(Problem));
            Problem[] array = (Problem[])Enum.GetValues(typeof(Problem));
            for (int i = 0; i < 50; i++)
            {
                if (names[i] == problem.ToString())
                {
                    return array[i];
                }
            }
            return Problem.None;
        }

        [RedirectMethod]
        public static Notification.ProblemStruct AddProblems(Notification.ProblemStruct problems1, Notification.ProblemStruct problems2)
        {
            for (int key = 0; key < 50; ++key)
            {
                Notification.Problem1 problem = ModInfo.ProblemTranslator[(Problem)key];
                if (ModInfo.Options.ProblemOptions[key] && (problems2 & problem) != null)
                    return (problems1 | problems2) & ~problem;
            }
            if ((problems2 & Notification.ProblemStruct.None) != null)
                return (problems1 & Notification.ProblemStruct.None) != null ? problems1 | problems2 : problems2;
            if ((problems2 & Notification.ProblemStruct.Major) != null)
            {
                if ((problems1 & Notification.ProblemStruct.None) != null)
                    return problems1;
                return (problems1 & Notification.ProblemStruct.Major) != null ? problems1 | problems2 : problems2;
            }

            // problems1 & -4611686018427387904L
            return (problems1 & Notification.ProblemStruct.Major) != null ? problems1 : problems1 | problems2;
        }
    }
}
