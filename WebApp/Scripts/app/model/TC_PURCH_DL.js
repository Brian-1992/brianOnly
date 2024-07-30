Ext.define('WEBAPP.model.TC_PURCH_DL', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PUR_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'AGEN_NAMEC', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'PUR_QTY', type: 'string' },
        { name: 'PUR_UNIT', type: 'string' },
        { name: 'IN_PURPRICE', type: 'string' },
        { name: 'PUR_AMOUNT', type: 'string' },


        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },

        { name: 'BASE_UNIT', type: 'string' },
        { name: 'M6AVG_USEQTY', type: 'string' },
        { name: 'INV_DAY', type: 'string' },
        { name: 'PURUN_MULTI', type: 'string' },
        { name: 'BASEUN_MULTI', type: 'string' },
        { name: 'PUR_DAY', type: 'string' },
        { name: 'ORI_AGEN_NAMEC', type: 'string' },
        { name: 'ORI_PUR_QTY', type: 'string' },
        { name: 'ORI_PUR_UNIT', type: 'string' },
        { name: 'ORI_IN_PURPRICE', type: 'string' },
        { name: 'ORI_PUR_AMOUNT', type: 'string' },


    ]
});