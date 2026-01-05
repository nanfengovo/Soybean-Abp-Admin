declare namespace Api {
  /**
   * namespace Auth
   *
   * backend api module: "auth"
   */
  namespace Auth {
    interface LoginToken {
      access_token: string;
      refresh_token: string;
    }

    interface User {
      userId: string;
      userName: string;
      // roles: string[];
      // buttons: string[];
    }

    interface UserInfo {
      userId: string;
      userName: string;
      roles: string[];
      // buttons: string[];
    }
  }
}
