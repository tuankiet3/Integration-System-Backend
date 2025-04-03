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
    }
}
