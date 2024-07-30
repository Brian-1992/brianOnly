Ext.define('WEBAPP.model.MeExpd', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'EXP_SEQ', type: 'string' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'EXP_DATE1', type: 'string' },
        { name: 'LOT_NO1', type: 'string' },
        { name: 'EXP_DATE2', type: 'string' },
        { name: 'LOT_NO2', type: 'string' },
        { name: 'EXP_DATE3', type: 'string' },
        { name: 'LOT_NO3', type: 'string' },
        { name: 'EXP_QTY', type: 'string' },
        { name: 'MEMO', type: 'string' },
        { name: 'REPLAY_DATE', type: 'string' },
        { name: 'REPLAY_ID', type: 'string' },
        { name: 'EXP_STAT', type: 'string' },
        { name: 'CLOSE_TIME', type: 'string' },
        { name: 'CLOSE_ID', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_ID', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_ID', type: 'string' },
        { name: 'IP', type: 'string' },

        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'WH_NAME', type: 'string' },
        { name: 'STORE_LOC', type: 'string' }
    ]
});