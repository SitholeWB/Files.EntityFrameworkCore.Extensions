namespace WebApi.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public DateTimeOffset LastModifiedDate { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
		public Guid? ImageId { get; set; }
	}
}