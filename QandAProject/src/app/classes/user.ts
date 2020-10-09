export class User {

  public authorizeToken: string;

  public login: string;

  constructor(login = '', authorizeToken = '') {
    this.login = login;
    this.authorizeToken = authorizeToken;
  }
}
