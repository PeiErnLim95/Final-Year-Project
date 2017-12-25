using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FYPFinal
{
    class Notification
    {
        public static void Notify()
        {
            // Generate the toast notification content
            ToastContent content = GenerateToastContent();
            //Pop the toast
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));

        }

        public static ToastContent GenerateToastContent()
        {
            return new ToastContent()
            {
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Reminder,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Cyberbully Content Detected"
                            },

                            new AdaptiveText()
                            {
                                Text = "Tweet Content"
                            },

                            new AdaptiveText()
                            {
                                Text = "Tweet Date"
                            },

                            new AdaptiveText()
                            {
                                Text = "Stream Time"
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {                    
                    Buttons =
                    {
                        new ToastButton("View","View")
                        {
                            //View the tweet record
                        },

                        new ToastButtonDismiss()
                    }
                }
            };
        }
    }
}