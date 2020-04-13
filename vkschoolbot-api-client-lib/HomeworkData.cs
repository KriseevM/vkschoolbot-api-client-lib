namespace SchoolBotAPI
{
    public class HomeworkData
    {
        public int ID { get; set; }
        public string Subject { get; set; }
        public string Homework { get; set; }
        public HomeworkData(int ID, string subject, string homework)
        {
            this.ID = ID;
            Subject = subject;
            Homework = homework;
        }
    }
}