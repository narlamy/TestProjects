﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Network
{
    /// <summary>
    /// 패킷의 그룹을 지정합니다.
    /// </summary>
    public class PacketGroupAttribute : Attribute
    {
        string mGroupName = "";

        public PacketGroupAttribute(string groupName)
        {
            mGroupName = groupName;
        }

        /// <summary>
        /// 그룹 이름 반환
        /// </summary>
        public string GroupName { get { return mGroupName; } }

        /// <summary>
        /// URI에 사용될 그룹 이름 문자열 (GroupName 뒤에 '/' 기호가 붙습니다.)
        /// </summary>
        public string UriGroupName { get { return mGroupName + "/"; } }
    }
}
