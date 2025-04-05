using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class PositionModel
    {
        [Required]
        private int positionId;
        public int PositionId { get { return positionId; } set { positionId = value; } }
        ///////////////////////////
        [Required]
        private string positionName;
        public string PositionName { get { return positionName; } set { positionName = value; } }
        ///////////////////////////
        private DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }
        ///////////////////////////
        private DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
    }
}
