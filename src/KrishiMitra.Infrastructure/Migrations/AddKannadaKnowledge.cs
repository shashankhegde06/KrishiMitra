using KrishiMitra.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KrishiMitra.Infrastructure.Migrations;

[DbContext(typeof(FarmDbContext))]
[Migration("202609280002_AddKannadaKnowledge")]
public sealed class AddKannadaKnowledge : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
INSERT INTO Knowledge (Id, Title, CropType, Language, Content, SourceName) VALUES
('activity-log-kn', 'ಕೃಷಿ ಚಟುವಟಿಕೆ ದಾಖಲಿಸುವುದು', 'General', 'kn', 'ಯಾವ ಕೆಲಸ ಮಾಡಲಾಯಿತು, ಯಾವ ದಿನಾಂಕದಂದು ಮಾಡಲಾಯಿತು ಮತ್ತು ತಿಳಿದಿದ್ದರೆ ಅದರ ಖರ್ಚನ್ನು ದಾಖಲಿಸಿ. ಕಂಡದ್ದನ್ನು ಮತ್ತು ಊಹಿಸಿದ್ದನ್ನು ಪ್ರತ್ಯೇಕವಾಗಿ ಬರೆಯಿರಿ.', 'ಸಾಮಾನ್ಯ ಶೈಕ್ಷಣಿಕ ಡೆಮೊ ಮಾಹಿತಿ'),
('report-problem-kn', 'ಬೆಳೆ ಸಮಸ್ಯೆಯನ್ನು ವರದಿ ಮಾಡುವುದು', 'General', 'kn', 'ಸಮಸ್ಯೆ ಯಾವಾಗ ಆರಂಭವಾಯಿತು, ಯಾವ ಗಿಡಗಳು ಅಥವಾ ಪ್ರದೇಶ ಬಾಧಿತವಾಗಿದೆ, ನೀವು ಏನು ಗಮನಿಸಿದ್ದೀರಿ ಮತ್ತು ಈಗಾಗಲೇ ಏನು ಕ್ರಮ ಕೈಗೊಂಡಿದ್ದೀರಿ ಎಂಬುದನ್ನು ದಾಖಲಿಸಿ. ಗಂಭೀರ ಸಮಸ್ಯೆಗೆ ಅರ್ಹ ಸ್ಥಳೀಯ ಕೃಷಿ ತಜ್ಞರನ್ನು ಸಂಪರ್ಕಿಸಿ.', 'ಸಾಮಾನ್ಯ ಶೈಕ್ಷಣಿಕ ಡೆಮೊ ಮಾಹಿತಿ'),
('expenses-kn', 'ಕೃಷಿ ಖರ್ಚು ದಾಖಲಿಸುವುದು', 'General', 'kn', 'ಪ್ರತಿ ಖರ್ಚಿನ ದಿನಾಂಕ, ಚಿಕ್ಕ ವಿವರಣೆ, ಮೊತ್ತ ಮತ್ತು ರಸೀದಿ ವಿವರವನ್ನು ದಾಖಲಿಸಿ. ಆಯ್ದ ಅವಧಿಯ ದಾಖಲೆಗಳನ್ನು ನೋಡಿ ಖರ್ಚುಗಳನ್ನು ಪರಿಶೀಲಿಸಿ.', 'ಸಾಮಾನ್ಯ ಶೈಕ್ಷಣಿಕ ಡೆಮೊ ಮಾಹಿತಿ');
");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DELETE FROM Knowledge WHERE Id IN ('activity-log-kn', 'report-problem-kn', 'expenses-kn');");
}
