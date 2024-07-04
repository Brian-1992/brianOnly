Ext.define('WEBAPP.model.CE0028', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'CHK_STATUS', type: 'string' }, //盤點狀態

        ///////////////找本次所有盤點項目(ALL)///////////////
        { name: 'CHK_NO_A', type: 'string' }, //盤點單號_A
        { name: 'MMCODE_A', type: 'string' }, //院內碼_A
        { name: 'MMNAME_C_A', type: 'string' }, //中文品名_A
        { name: 'MMNAME_E_A', type: 'string' }, //英文品名_A
        { name: 'BASE_UNIT_A', type: 'string' }, //計量單位代碼_A
        { name: 'STORE_QTY_A', type: 'string' }, //總電腦量_A
        { name: 'CHK_QTY1_A', type: 'string' }, //盤點量(初)_A
        { name: 'CHK_QTY2_A', type: 'string' }, //盤點量(複)_A
        { name: 'CHK_QTY3_A', type: 'string' }, //盤點量(三)_A
        { name: 'CHK_QTY_A', type: 'string' }, //盤點量_A
        { name: 'CHK_UID_NAME_A', type: 'string' }, //盤點人員_A
        { name: 'STATUS_TOT_A', type: 'string' }, //盤點階段(1:初盤, 2.複盤, 3:三盤)_A
        { name: 'GAP_T_A', type: 'string' }, //總誤差量_A

        ///////////////用初盤chk_no找三盤的項目 ///////////////
        { name: 'CHK_NO_3', type: 'string' }, //盤點單號_3
        { name: 'MMCODE_3', type: 'string' }, //院內碼_3
        { name: 'MMNAME_C_3', type: 'string' }, //中文品名_3
        { name: 'MMNAME_E_3', type: 'string' }, //英文品名_3
        { name: 'BASE_UNIT_3', type: 'string' }, //計量單位_3
        { name: 'CHK_QTY_3', type: 'string' }, //盤點量_3
    ]
});