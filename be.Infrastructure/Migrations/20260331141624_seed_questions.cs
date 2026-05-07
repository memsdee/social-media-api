using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini4rum.Migrations
{
    /// <inheritdoc />
    public partial class seed_questions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                TRUNCATE TABLE questions.questions RESTART IDENTITY CASCADE;

                WITH formats AS (
                    SELECT UNNEST(ARRAY[
                        'Mọi người nghĩ sao về vấn đề %s trong thời đại này?',
                        'Bài học lớn nhất mình rút ra sau khi trải qua %s.',
                        'Góc tâm sự: Mình đang rất bế tắc với %s...',
                        'Theo bạn, %s mang lại lợi ích hay tác hại nhiều hơn?',
                        'Có phải chúng ta đang làm quá lên về %s?',
                        'Sức ảnh hưởng của %s đến quyết định hàng ngày của bạn.',
                        'Có thực sự tồn tại sự công bằng trong %s?',
                        'Lý do vì sao mình quyết định từ bỏ %s.',
                        'Làm thế nào để tìm được sự bình yên trong %s?',
                        'Bạn hối hận nhất điều gì khi nghĩ về %s?',
                        'Điều khiến bạn tự hào nhất khi nhắc về %s của bản thân.',
                        'Làm thế nào để không bị so sánh với người khác trong %s?',
                        'Giá trị lớn lao nhất mà %s đem lại cho bạn là gì?',
                        'Bạn có sẵn lòng bắt đầu lại từ số không với %s?',
                        'Góc tranh luận: %s có đáng để chúng ta hy sinh nhiều như vậy?'
                    ]) AS fmt
                ),
                topics AS (
                    SELECT UNNEST(ARRAY[
                        'tình yêu', 'công việc', 'tiền bạc', 'gia đình', 'tình bạn',
                        'sức khỏe', 'đam mê bản thân', 'mạng xã hội', 'cuộc sống hôn nhân', 'việc học hành'
                    ]) AS topic
                )
                INSERT INTO questions.questions (content)
                SELECT format(fmt, topic)
                FROM formats
                CROSS JOIN topics;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE questions.questions RESTART IDENTITY CASCADE;");

        }
    }
}
