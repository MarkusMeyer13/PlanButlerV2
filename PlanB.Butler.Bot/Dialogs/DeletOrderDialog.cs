﻿namespace ButlerBot
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using BotLibraryV2;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    public class DeleteOrderDialog : ComponentDialog
    {
        static Plan plan = new Plan();
        static Plan orderedfood = new Plan();
        static int valueDay;
        const double grand = 3.30;
        static string dayName;
        static string[] weekDays = { "Montag", "Dienstag", "Mitwoch", "Donnerstag", "Freitag" };
        static string[] weekDaysEN = { "monday", "tuesday", "wednesday", "thursday", "friday" };
        static int indexer = 0;
        static string[] companyStatus = { "intern", "extern", "internship" };
        static string[] companyStatusD = { "Für mich", "Kunde", "Praktikant" };
        static Order obj = new Order();


        public DeleteOrderDialog()
            : base(nameof(DeleteOrderDialog))
        {
            // Get the Plan
            string food = BotMethods.GetDocument("eatingplan", "ButlerOverview.json");
            
            plan = JsonConvert.DeserializeObject<Plan>(food);
            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
                {
                this.InitialStepAsync,
                CompanyStepAsync,
                this.NameStepAsync,
                RemoveStepAsync,
                DeleteOrderStep,
                };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new NextOrder());

            // The initial child Dialog to run.
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();
            List<string> currentWeekDays = new List<string>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);


            for (int i = indexer; i < weekDays.Length; i++)
            {
                currentWeekDays.Add(weekDays[i]);
            }

            if (currentWeekDays != null)
            {
                return await stepContext.PromptAsync(
                    nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"Wann möchtest du deine bestellung löschen?"),
                        Choices = ChoiceFactory.ToChoices(currentWeekDays),
                        Style = ListStyle.HeroCard,
                    }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Tut mir Leid. Ich habe dich nicht verstanden. Bitte benutze Befehle, die ich kenne."), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> CompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["mainChoise"] = ((FoundChoice)stepContext.Result).Value;
            string text = stepContext.Values["mainChoise"].ToString();
            for (int i = 0; i < weekDays.Length; i++)
            {
                if (weekDays[i] == text)
                {
                    indexer = i;
                }
                else if (weekDays[i] == text && weekDaysEN[i] != "friday")
                {
                    indexer = i + 1;
                }
            }

            stepContext.Values["name"] = stepContext.Context.Activity.From.Name;
            return await stepContext.PromptAsync(
               nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Für wen willst du bestellen?"),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "Für mich", "Praktikant", "Kunde" }),
                   Style = ListStyle.HeroCard,
               }, cancellationToken);
        }


        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["companyStatus"] = ((FoundChoice)stepContext.Result).Value;
            for (int i = 0; i < companyStatusD.Length; i++)
            {
                if (stepContext.Values["companyStatus"].ToString() == companyStatusD[i])
                {
                    stepContext.Values["companyStatus"] = companyStatus[i];
                }
            }

            valueDay = plan.Planday.FindIndex(x => x.Name == weekDaysEN[indexer]);
            dayName = weekDaysEN[indexer];
            stepContext.Values["name"] = stepContext.Context.Activity.From.Name;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> RemoveStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var order = new Order();
                order.CompanyStatus = stepContext.Values["companyStatus"].ToString();
                order.Name = (string)stepContext.Values["name"].ToString();
                List<Order> mealVal = new List<Order>();
                OrderBlob orderBlob = new OrderBlob();
                string[] weekDaysList = { "monday", "tuesday", "wednesday", "thursday", "friday" };
                int indexDay = 0;
                int indexCurentDay = 0;
                string currentDay = DateTime.Now.DayOfWeek.ToString().ToLower();
                DateTime date = DateTime.Now;
                var stringDate = string.Empty;
                for (int i = 0; i < weekDaysList.Length; i++)
                {
                    if (currentDay == weekDaysList[i])
                    {
                        indexCurentDay = i;
                    }
                    if (weekDaysEN[indexer] == weekDaysList[i])
                    {
                        indexDay = i;
                    }
                }
                if (indexDay == indexCurentDay)
                {
                    stringDate = date.ToString("yyyy-MM-dd");
                }
                else
                {
                    indexCurentDay = indexDay - indexCurentDay;
                    date = DateTime.Now.AddDays(indexCurentDay);
                    stringDate = date.ToString("yyyy-MM-dd");
                }
                orderBlob = JsonConvert.DeserializeObject<OrderBlob>(BotMethods.GetDocument("orders", "orders_" + stringDate + "_" + order.Name + ".json"));
                var collection = orderBlob.OrderList.FindAll(x => x.Name == order.Name);
                obj = collection.FindLast(x => x.CompanyStatus == order.CompanyStatus);

                return await stepContext.PromptAsync(
                    nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"Soll {obj.Meal}  gelöscht werden?"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "Ja", "Nein" }),
                        Style = ListStyle.HeroCard,
                    }, cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"An diesem Tag gibt es keine Bestellung.\n:("), cancellationToken);
                await stepContext.EndDialogAsync(null, cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(OverviewDialog), null, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> DeleteOrderStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["choise"] = ((FoundChoice)stepContext.Result).Value;
            var text = stepContext.Values["choise"];
            if (text.ToString().ToLower() == "ja")
            {
                var bufferOrder = obj;
                var order = bufferOrder;

                DeleteOrder(order);

                DeleteOrderforSalaryDeduction(bufferOrder);
                BotMethods.DeleteMoney(bufferOrder, weekDaysEN[indexer]);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Okay deine Bestellung wurde entfernt"), cancellationToken);
                await stepContext.EndDialogAsync();
                return await stepContext.BeginDialogAsync(nameof(OverviewDialog));
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Okay deine Bestellung wurde entfernt."), cancellationToken);
                await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(OverviewDialog), null, cancellationToken);
            }
        }

        public static Order GetOrder(Order order)
        {
            OrderBlob orderBlob = new OrderBlob();
            int weeknumber = (DateTime.Now.DayOfYear / 7) + 1;
            orderBlob = JsonConvert.DeserializeObject<OrderBlob>(BotMethods.GetDocument("orders", "orders_" + weeknumber + "_" + DateTime.Now.Year + ".json"));
          
            var bufferOrder = orderBlob.OrderList.FindAll(x => x.Name == order.Name);

            var temp = bufferOrder.FindAll(x => x.CompanyStatus == order.CompanyStatus);
            var orderValue = temp[temp.Count - 1];
            return orderValue;
        }
        public static void DeleteOrder(Order order)
        {
            string date = order.Date.ToString("yyyy-MM-dd");
            OrderBlob orderBlob = new OrderBlob();
            int weeknumber = (DateTime.Now.DayOfYear / 7) + 1;
            try
            {
                orderBlob = JsonConvert.DeserializeObject<OrderBlob>(BotMethods.GetDocument("orders", "orders_" + date + "_" + order.Name + ".json"));
                int orderID = orderBlob.OrderList.FindIndex(x => x.Name == order.Name);
                orderBlob.OrderList.RemoveAt(orderID);
                BotMethods.PutDocument("orders", "orders_" + date + "_" + order.Name + ".json", JsonConvert.SerializeObject(orderBlob), "q.planbutlerupdateorder");
            }
            catch (Exception ex) // enters if blob dont exist
            {
                List<Order> orders = new List<Order>();

                orders.Add(order);
                BotMethods.PutDocument("orders", "orders_" + date + "_" + order.Name + ".json", JsonConvert.SerializeObject(orderBlob), "q.planbutlerupdateorder");
            }
        }
        public static void DeleteOrderforSalaryDeduction(Order order)
        {
            SalaryDeduction salaryDeduction = new SalaryDeduction();
            var dayId = order.Date.Date.DayOfYear;
            salaryDeduction = JsonConvert.DeserializeObject<SalaryDeduction>(BotMethods.GetDocument("salarydeduction", "orders_" + dayId.ToString() + "_" + DateTime.Now.Year + ".json"));
            var collection = salaryDeduction.Order.FindAll(x => x.Name == order.Name);
            var temp = collection.FindAll(x => x.CompanyStatus == order.CompanyStatus);
            salaryDeduction.Order.Remove(temp[temp.Count - 1]);

            try
            {
                BotMethods.PutDocument("salarydeduction", "orders_" + dayId.ToString() + "_" + DateTime.Now.Year.ToString() + ".json", JsonConvert.SerializeObject(salaryDeduction), "q.planbutlerupdatesalary");
            }
            catch // enters if blob dont exist
            {
                List<Order> orders = new List<Order>();

                salaryDeduction.Daynumber = dayId;
                salaryDeduction.Name = "SalaryDeduction";

                orders.Add(order);
                salaryDeduction.Order = orders;

                BotMethods.PutDocument("salarydeduction", "orders_" + dayId.ToString() + "_" + DateTime.Now.Year.ToString() + ".json", JsonConvert.SerializeObject(salaryDeduction), "q.planbutlerupdatesalary");
            }
        }
    }
}