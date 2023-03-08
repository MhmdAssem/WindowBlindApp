import { UserSettings } from './userSettings';

export class User {
  id = '';
  name = '';
  password = '';
  role = '';
  settings = new UserSettings();
}
