#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressManager
    {
        public event EventHandler<ProgressEventArgs> Progress;

        public ProgressContext Create(string displayText, object context)
        {
            ProgressContext progress = new ProgressContext(displayText, context);
            progress.Progressing += new EventHandler<ProgressEventArgs>(progress_Progressing);

            return progress;
        }

        public ProgressContext Create(string displayText)
        {
            return Create(displayText, null);
        }

        private void progress_Progressing(object sender, ProgressEventArgs e)
        {
            OnProgress(e);
        }

        protected virtual void OnProgress(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = Progress;
            if (handler != null)
            {
                handler(null, e);
            }
        }
    }
}