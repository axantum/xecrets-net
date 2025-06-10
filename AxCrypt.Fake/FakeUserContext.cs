using System;
using System.Linq;

#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

namespace AxCrypt.Fake
{
    public class FakeUserContext
    {
        private string _userEmail; // Add a private field to store the user's email

        public string Name
        {
            get { return _userEmail; } // Return the configured user's email
        }

        public FakeUserContext(string userEmail)
        {
            ConfigureUserEmail(userEmail);
        }

        public void ConfigureUserEmail(string userEmail)
        {
            // Set the email that will be returned by the Name property
            _userEmail = userEmail;
        }
    }
}