using am.BL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Threading;
using System.Reflection;

namespace Budget.Tools
{
    public class Backup
    {
        public static event Action<string> OnProgress;
        static void Progress(string msg, int t = 1000) { OnProgress?.Invoke(msg); Thread.Sleep(t); }
        static void Command(int cmd) { OnProgress?.Invoke(cmd.ToString()); }
        public static void SaveToJson(string filePath)
        {
            JObject backupJsonObject = new JObject();

            Progress("Сохраняем пользователя ...");
            //Сохранить пользователя
            DataTable dt = G.db_select($"select * from Users where ID = {U.Cur.ID}");
            string userJsonString = JsonConvert.SerializeObject(dt);
            backupJsonObject.Add("User", JToken.Parse(userJsonString)[0]);
            JToken user = backupJsonObject["User"];
            Progress("готово");

            Progress("Сохраняем Счета ...");
            //Сохранить все мои Счета 
            dt = G.db_select($"select * from Accounts where UserID = {U.Cur.ID}");
            string accountsJsonString = JsonConvert.SerializeObject(dt);
            //Мои счета сохраняю как объект внутри Юзера
            user["Accounts"] = JToken.Parse(accountsJsonString);
            Progress("готово");

            Progress("Сохраняем Категории ...");
            //Сохранить все мои Категории
            dt = G.db_select($"select * from Categories where UserID = {U.Cur.ID}");
            string categoriesJsonString = JsonConvert.SerializeObject(dt);
            user["Categories"] = JToken.Parse(categoriesJsonString);
            Progress("готово");

            Progress("Сохраняем операции ...");
            //Сохранить все операции по всем моим счетам
            string sql = $@"
                select distinct o.* from Operations o 
                join Accounts a on a.ID in (Debet_ID, Credit_ID) where UserID = {U.Cur.ID}
            ";
            dt = G.db_select(sql);
            string operationsJsonString = JsonConvert.SerializeObject(dt);
            user["Operations"] = JToken.Parse(operationsJsonString);
            Progress("готово");

            Progress("Сохраняем остатки ...");
            //Сохранить остатки по всем моим счетам, можно даже не делать этого, так как есть операции, но на всякий
            //При восстановлении, можно проверить остатки из операций с остатками в сохраненном бэкапе
            dt = G.db_select($"select r.* from Acc_Rests r join Accounts a on a.ID = r.Account_ID where UserID = {U.Cur.ID}");
            string accRestsJsonString = JsonConvert.SerializeObject(dt);
            user["Acc_Rests"] = JToken.Parse(accRestsJsonString);
            Progress("готово");

            Progress("Записываем в файл ...");
            File.WriteAllText(filePath, backupJsonObject.ToString());
            Progress("Бэкап готов!");
            Progress("0");
        }
        public static bool RestoreFromJson(string filePath)
        {
            string error = "";
            JObject backupJsonObject = null;

            try
            {
                Progress("Загружаю файл бэкапа в пямять ...");
                //Загрузить и распарсить файл бэкапа        
                backupJsonObject = JObject.Parse(File.ReadAllText(filePath));
                Progress("готово");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error parse file.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                Progress("Получаю пользователя из бэкапа ...");
                //Получить корневой элемент User 
                JToken user = backupJsonObject["User"]; 
                //Получить UserID из файла бэкапа, кто сохранил бэкап
                int UserID = (int)user["ID"];
                bool IsNewUser = U.Cur.ID != UserID;    
                Progress("готово");

                Command(2/*Скрыть окошко прогресса*/);
                //Вместо прогресса показать окно выбора дальнейших действий

                var msg = IsNewUser ? 
                    "Бэкап был создан под другим пользователем!\n"+
                    "Для востановления из бэкапа все ваши данные будут удалены!\n"+
                    "Все Счета, Категории и операции будут взяты из бэкапа, ваших больше не будет.\n"+
                    "Вы согласны? Продолжить?"
                    : 
                    "Ваши счета, категории и операции будут обновлены!\n"+
                    "В бэкапе могут быть другие названия Счетов и Категорий,\n"+
                    "так же могут измениться описания, категории и суммы операций.\n"+
                    "Могут добавиться удаленные Счета, Категории и операции!\n"+
                    "Сущности, которых нет в бэкапе, но есть у вас не удалятся.\n"+
                    "Продолжить?";
              
                if (MessageBox.Show(msg, "Бюджет " + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    Command(0/*Закрыть прогресс окончательно*/);
                    return false;
                }

                try
                {
                    Command(3/*Показать скрытое окно прогресса*/);

                    if (IsNewUser)
                    {
                        Progress("Удаляю все ваши Счета, Категории и Операции ...");
                        G.db_exec($"am_ClearAllUserData {U.Cur.ID}");
                        Progress("готово");
                    }
                    Progress("Получаю Счета, Категории и Операции из бэкапа ...");

                    JToken accounts = user["Accounts"];
                    JToken categors = user["Categories"];
                    JToken operatis = user["Operations"]; Progress("готово");

                    Progress("Обрабатываю Счета ...");
                    if (!EF.Accounts.Restore(accounts, out error))
                    {
                        MessageBox.Show(error, "Error restore Accounts.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    Command(1); Progress("готово");

                    Progress("Обрабатываю Категории ...");
                    if (!EF.Categors.Restore(categors, out error))
                    {
                        MessageBox.Show(error, "Error restore Categories.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    Progress("готово");

                    Progress("Обрабатываю Операции ...");
                    EF.Operatis.accIdsMap = EF.Accounts.idsMap;
                    EF.Operatis.catIdsMap = EF.Categors.idsMap;
                    EF.Operatis.IsNewUser = IsNewUser;

                    if (!EF.Operatis.Restore(operatis, out error))
                    {
                        MessageBox.Show(error, "Error restore Operations.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    Command(1); Progress("готово");

                    Progress("Пересчет остатков ...");
                    if(!EF.Accounts.RecalcRests(out error))
                    {
                        MessageBox.Show(error, "Error recalculating rests.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    Command(1); Progress("готово");

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error load from User.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error load User.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Progress("Восстановление из бэкапа завершено!", 3000);
            Command(1/*Обновить данные в гридах счетов и операций*/); 
            Command(0);

            return true;
        }

    }
}
