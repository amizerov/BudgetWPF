using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Budget")]
[assembly: AssemblyDescription("Manage your family budget and amass money for the future.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ultrazoom, LLC")]
[assembly: AssemblyProduct("Budget")]
[assembly: AssemblyCopyright("Copyright © 2015")]
[assembly: AssemblyTrademark("UltraZoomBudget")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.3.2.0")]
/*TODO***********************************************
 * -----------------------------------------
 * Ошибки:
 *      1 - В таскбаре окно пересчета остатков не надо
 *      2 - Сохранение темы при смене, восстановление сохраненной при открытии проги
 *      3 - Сохранение лайаута таблиц без закрытия
 *      4 - Ордер первого в списке счета всегда растет быстрее других
 * Новые хотелки:
 *      1 - сделать нормальный отчет по категориям
 *      2 - убрать редактирование комбобокса категорий
 *      3 - Шифровать файл бэкапа
 *      4 - Проверить бэкап по остаткам
 *      5 - Добавить редактирование ордера счета
 * Старые хотелки:
 *      1 - Смена пароля
 *      2 - Восстановление пароля
 *      3 - Ентер в поле Описание на форме Операции переводит строку и жмет на Сохранить, оставить что-то одно
 *      4 - Отчеты - Счета - Оптимизация процедуры
 *      5 - При двойном клике на строку отчета проваливаться в список операций
 *      6 - Отчет по расходам в виде пирога
 *      7 - Вынести тексты для Шаринга и О программе в базу данных.
 *      8 - Автоматизировать обновление версии
 *      9 - Кнопка редактировать счет не работает
 *      
 * **ПЛАТНАЯ ВЕРСИЯ**********************************     
 * В платнгой версии доступны функции:
 *      1 - Добавление помошника
 * **************************************************     
 * 
 * ИСТОРИЯ ВЕРСИЙ:
 *  09/11/2022 Версия 1.3.1.8
 * -------------------------------
 * +    1 - Сохранение состояния гридов на главном окне
 * +    2 - График общего баланса и расходов
 * +    3 - Суммы остатков и оборотов в гридах Счетов и Операций
 * +    4 - Пересчитываются остатки при смене статуса
 * +    5 - Создание и восстановление из бэкапа
 * +    6 - Исправлена ошибка с новыми темами 
 * +    7 - Исправлена ошибка при смене темы в WinForms UserControl
 * -------------------------------
 *  18/10/2022 Версия 1.3.0.7
 * -------------------------------
 * +    1 - Update DevExpress to 21.2
 * -------------------------------
 *  08/08/2015 Версия 1.3.0.6
 * -------------------------------
 * +    1 - Исправлена ошибка сборки, нехватало DevExpress.Xpf.LayoutControl
 * -------------------------------
 * -------------------------------
 *  06/03/2015 Версия 1.3.0.5
 * -------------------------------
 * +    1 - Добавление ПОМОШНИКА
 * -------------------------------
 *  04/03/2015 Версия 1.3.0.4
 * -------------------------------
 * +    1 - Починить загрузку чеков
 * +    2 - Форму EditOperation сделать как AddOperation или совместить их
 * +    3 - При удалении операции просить подтверждения
 * -------------------------------
 *  24/02/2015 Версия 1.3.0.3
 * -------------------------------
 * +    1 - Отчеты: Категории: Оптимизация процедуры
 * +    2 - Загрузка операций САЙМАК из PARKING
 * +    3 - Окно "О программе" по кнопке F1
 * +    4 - Убрать из трая Окно "О программе"
 * +    5 - Закрывать Окно "О программе" по Esc
 * +    6 - В Окне "О программе" фокус сразу на кнопке Закрыть

*/