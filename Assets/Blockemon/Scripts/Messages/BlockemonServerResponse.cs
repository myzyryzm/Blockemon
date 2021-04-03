using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeControl;

namespace Blockemon.Messages
{
    public class BlockemonServerResponse : Message
    {
        public List<Blockemon> blockemons;

        public BlockemonServerResponse() {}

        public BlockemonServerResponse(List<Blockemon> blockemons)
        {
            this.blockemons = blockemons;
        }
    }
}

