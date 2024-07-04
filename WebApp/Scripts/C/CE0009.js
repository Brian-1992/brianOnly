﻿﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {

    //#region (20200212~)

    //#region params
    var chk_period = 'D';
    var chk_level = '3';
    var chk_ym = getDefaultDateValue();
    var chk_uid_count = 0;

    var wh_kind = '';
    var sort_by = '';
    var sort_order = 'asc';
    var showGrid = '';

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    console.log('windowHeight', windowHeight);

    var msgCountGet = '/api/CE0003/GetMsgCount';
    var T1Update = '/api/CE0003/UpdateCE0003_INI';
    var FinalPro = '/api/CE0003/FinalPro';  //完成盤點
    var St_PeopleGet = '/api/CE0003/GetPeopleCombo';
    //#endregion

    //#region combos, models, stores

    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'MMNAME_C', 'STORE_QTYC', 'CHK_QTY,', 'STORE_LOC', 'BASE_UNIT', 'CHK_TIME', 'BARCODE']
    });

    var chknoStore = Ext.create('Ext.data.Store', {
        fields: ['CHK_NO', 'CHK_YM', 'CHK_CLASS_NAME', 'CHK_TYPE_NAME', 'CHK_STATUS_NAME', 'DISPLAY', 'CHK_UID_COUNT']
    });
    var chkTypeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME', 'WH_KIND', 'WH_GRADE']
    });

    // 此盤點單的盤點人員清單
    var st_people = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setChknoComboData() {
        //盤點人員
        Ext.Ajax.request({
            url: '/api/CE0003/GetChknoCombo/',
            method: reqVal_p,
            params:
            {
                chk_level: chk_level,
                chk_period: chk_period,
                chk_ym: chk_period == 'D' ? T1Query.getForm().findField('CHK_YM_DATE').getValue() : T1Query.getForm().findField('CHK_YM_MONTH').getValue(),
                wh_no: T1Query.getForm().findField('WH_NO').getValue(),
                chk_type: T1Query.getForm().findField('CHK_TYPE').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    chknoStore.removeAll();
                    var chknos = data.etts;
                    if (chknos.length > 0) {
                        for (var i = 0; i < chknos.length; i++) {
                            chknos[i].DISPLAY = (chknos[i].CHK_TYPE_NAME);
                            chknoStore.add(chknos[i]);
                        }
                        Ext.getCmp('btnStart').disable();
                    } else {
                        var chk_level_name = '';
                        if (chk_level == '1') { chk_level_name = '初盤' };
                        if (chk_level == '2') { chk_level_name = '複盤' };
                        if (chk_level == '3') { chk_level_name = '三盤' };

                        var chk_type = '';
                        chk_type = T1Query.getForm().findField('CHK_TYPE').rawValue;

                        Ext.Msg.alert('提醒', '無需盤點之' + chk_level_name + ' ' + chk_type + ' 盤點單');
                        Ext.getCmp('btnStart').disable();
                    }
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });

    }

    function setWhnoComboData() {
        Ext.Ajax.request({
            url: '/api/CE0003/GetWhnoCombo/',
            method: reqVal_p,
            params: {
                chk_level: chk_level,
                chk_period: chk_period,
                chk_ym: chk_period == 'D' ? T1Query.getForm().findField('CHK_YM_DATE').getValue() : T1Query.getForm().findField('CHK_YM_MONTH').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    whnoStore.removeAll();
                    var whnos = data.etts;
                    if (whnos.length > 0) {
                        for (var i = 0; i < whnos.length; i++) {
                            whnos[i].DISPLAY = whnos[i].WH_NO + ' ' + whnos[i].WH_NAME;
                            whnoStore.add(whnos[i]);
                        }
                        T1Query.getForm().findField('WH_NO').setValue(whnos[0].WH_NO);
                        wh_kind = whnos[0].WH_KIND;

                        setChkTypeComboData();
                    } else {
                        var msg = '無需' + (chk_period == 'D' ? '日' : '月') + '盤之庫房';
                        Ext.Msg.alert('提醒', msg);
                        Ext.getCmp('btnStart').disable();
                    }
                }
            },
            failure: function (response, options) {
            }
        });

    }
    function setChkTypeComboData() {
        //盤點人員
        Ext.Ajax.request({
            url: '/api/CE0003/GetChkTypeCombo/',
            method: reqVal_p,
            params:
            {
                chk_level: chk_level,
                chk_period: chk_period,
                chk_ym: chk_period == 'D' ? T1Query.getForm().findField('CHK_YM_DATE').getValue() : T1Query.getForm().findField('CHK_YM_MONTH').getValue(),
                wh_no: T1Query.getForm().findField('WH_NO').getValue(),
                wh_kind: wh_kind
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    chkTypeStore.removeAll();
                    var chkTypes = data.etts;
                    if (chkTypes.length > 0) {
                        for (var i = 0; i < chkTypes.length; i++) {
                            chkTypeStore.add(chkTypes[i]);
                        }
                        Ext.getCmp('btnStart').disable();
                        T1Query.getForm().findField('CHK_TYPE').setValue(chkTypes[0].VALUE);
                        setChknoComboData();

                    } else {
                        var chk_level_name = '';
                        if (chk_level == '1') { chk_level_name = '初盤' };
                        if (chk_level == '2') { chk_level_name = '複盤' };
                        if (chk_level == '3') { chk_level_name = '三盤' };
                        Ext.Msg.alert('提醒', '無需盤點之' + chk_level_name + ' ' + chkTypes[0].TEXT + ' 盤點單');
                        Ext.getCmp('btnStart').disable();
                    }
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });

    }

    function setChkuidComboData() {
        //盤點人員
        Ext.Ajax.request({
            url: St_PeopleGet,
            method: reqVal_p,
            params:
            {
                chk_no: T1Query.getForm().findField('CHK_NO').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    st_people.removeAll();
                    var tb_people = data.etts;
                    if (tb_people.length > 0) {

                        var UserId = session['UserId'];
                        for (var i = 0; i < tb_people.length; i++) {
                            st_people.add({ VALUE: tb_people[i].VALUE, TEXT: tb_people[i].TEXT });
                        }

                        for (var i = 0; i < tb_people.length; i++) {
                            //初始化
                            if (tb_people[i].VALUE == UserId) {
                                T21Query.getForm().findField('CHK_UID').setValue(tb_people[i].VALUE);
                            }
                        }
                    }
                    T21Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });

    }
    //#endregion

    function getFirstItem(mmcode, store_loc) {
        Ext.Ajax.request({
            url: '/api/CE0003/getFirstItem/',
            method: reqVal_p,
            params:
            {
                sort_by: sort_by,
                sort_order: sort_order,
                chk_no: T1Query.getForm().findField('CHK_NO').getValue(),
                mmcode: mmcode,
                store_loc: store_loc,
                chk_uid: chk_uid_count == 0 ? T21Query.getForm().findField('CHK_UID').getValue() : ''
            },
            success: function (response) {

                console.log(response);
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    T21Query.getForm().findField('BARCODE').setValue('');
                    T21Query.getForm().findField('BARCODE').focus();
                    if (data.etts.length == 0) {
                        T11Load();
                        changeCardT11();
                        updateMsgCount();
                        return;
                    }

                    var r = Ext.create('WEBAPP.model.CE0003');
                    r.set(data.etts[0]);
                    itemDetail.loadRecord(r);

                    Ext.getCmp('btn_confirm').disable();
                    itemDetail.getForm().findField('CHK_QTY').disable();
                    // 藥庫預設盤點量為電腦量
                    if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S')
                        itemDetail.getForm().findField('CHK_QTY').setValue(data.etts[0]['STORE_QTYC']);
                    setTimeout(function () {
                        T21Query.getForm().findField('BARCODE').focus();

                        if (itemDetail.getForm().findField('STORE_QTYC').getValue() == '0') {
                            itemDetail.getForm().findField('CHK_QTY').enable();
                            itemDetail.getForm().findField('STORE_LOC').focus();
                            Ext.getCmp('btn_confirm').enable();
                        }
                        //itemDetail.getForm().findField('CHK_QTY').focus();
                    }, 300);

                    updateMsgCount();
                    //itemDetail.loadRecord(data.etts[0]);
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        //collapsible: true,
        //hideCollapseTool: true,
        //titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 90,
            //labelStyle: 'width: 25%',
            width: '100%',
            labelStyle: "font-size:20px;font-weight:bold;",
            fieldStyle: "font-size:20px",
        },

        items: [{
            xtype: 'container',
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'radiogroup',
                    fieldLabel: '盤點期',
                    name: 'CHK_PERIOD',
                    columns: 2,
                    items: [
                        {
                            boxLabel: '<span style="font-size:20px">日盤</span>', name: 'CHK_PERIOD', inputValue: 'D', checked: true,
                        },
                        { boxLabel: '<span style="font-size:20px">月盤</span>', name: 'CHK_PERIOD', inputValue: 'M' },
                    ], listeners: {
                        change: function (field, newValue, oldValue) {
                            Ext.getCmp('chkStatusWarning').hide();
                            T1Query.getForm().findField('CHK_NO').setValue('');
                            Ext.getCmp('grid_sort').hide();
                            chk_period = newValue['CHK_PERIOD'];
                            if (chk_period == 'D') {
                                chk_ym = T1Query.getForm().findField('CHK_YM_DATE').getValue();
                                T1Query.getForm().findField('CHK_YM_DATE').show();
                                T1Query.getForm().findField('CHK_YM_MONTH').hide();
                            } else {
                                chk_ym = T1Query.getForm().findField('CHK_YM_MONTH').getValue();
                                T1Query.getForm().findField('CHK_YM_MONTH').show();
                                T1Query.getForm().findField('CHK_YM_DATE').hide();
                            }
                            T1Query.getForm().findField('WH_NO').setValue('');
                            setWhnoComboData();
                            //T1Query.getForm().findField('CHK_NO').setValue('');
                            //Ext.getCmp('btnStart').disable();
                        }
                    }
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '日期',
                    name: 'CHK_YM_DATE',
                    value: getDefaultDateValue()
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '月份',
                    name: 'CHK_YM_MONTH',
                    value: getDefaultMonthValue(),
                    hidden: true
                },
                {
                    xtype: 'combo',
                    store: whnoStore,
                    fieldLabel: '庫房',
                    name: 'WH_NO',
                    displayField: 'DISPLAY',
                    valueField: 'WH_NO',
                    queryMode: 'local',
                    autoSelect: true,
                    fieldCls: 'required',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;"><span style="font-size:20px">{DISPLAY}</span></div></tpl>',
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {
                            T1Query.getForm().findField('CHK_TYPE').setValue('');
                            T1Query.getForm().findField('CHK_NO').setValue('');
                            Ext.getCmp('grid_sort').hide();
                            setChkTypeComboData();

                            //Ext.getCmp('btnStart').disable();
                            //Ext.getCmp('chkStatusWarning').hide();
                            // Ext.getCmp('chkStatusWarning').show();
                        },
                        focus: function (field, event, eOpts) {

                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 100);
                            }
                        }
                    }
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點階段',
                    value: '三盤'
                },
                //{
                //    xtype: 'radiogroup',
                //    fieldLabel: '盤點階段',
                //    name: 'CHK_LEVEL',
                //    columns: 2,
                //    items: [
                //        {
                //            boxLabel: '<span style="font-size:20px">初盤</span>', name: 'CHK_LEVEL', inputValue: '1', checked: true,
                //        },
                //        { boxLabel: '<span style="font-size:20px">複盤</span>', name: 'CHK_LEVEL', inputValue: '2' },
                //        ,
                //        { boxLabel: '<span style="font-size:20px">三盤</span>', name: 'CHK_LEVEL', inputValue: '3' }
                //    ], listeners: {
                //        change: function (field, newValue, oldValue) {
                //            T1Query.getForm().findField('CHK_NO').setValue('');
                //            Ext.getCmp('grid_sort').hide();
                //            Ext.getCmp('chkStatusWarning').hide();
                //            chk_level = newValue['CHK_LEVEL'];
                //            //setComboData();
                //            Ext.getCmp('btnStart').disable();
                //            setChknoComboData();
                //        }
                //    }
                //},
                {
                    xtype: 'combo',
                    store: chkTypeStore,
                    fieldLabel: '盤點類別',
                    name: 'CHK_TYPE',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    fieldCls: 'required',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;"><span style="font-size:20px">{TEXT}&nbsp;</span></div></tpl>',
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {

                            T1Query.getForm().findField('CHK_NO').setValue('');
                            Ext.getCmp('grid_sort').hide();
                            setChknoComboData();

                        },
                        focus: function (field, event, eOpts) {

                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 100);
                            }
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: chknoStore,
                    fieldLabel: '盤點單',
                    name: 'CHK_NO',
                    displayField: 'CHK_NO',
                    valueField: 'CHK_NO',
                    queryMode: 'local',
                    autoSelect: true,
                    fieldCls: 'required',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;"><span style="font-size:20px">{CHK_NO}&nbsp;</span></div></tpl>',
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {
                            chk_uid_count = Number(r.data.CHK_UID_COUNT);

                            Ext.getCmp('btnStart').disable();
                            Ext.getCmp('chkStatusWarning').hide();
                            Ext.getCmp('grid_sort').hide();
                            if (r.data.CHK_STATUS == '1') {
                                Ext.getCmp('grid_sort').show();
                                Ext.getCmp('btnStart').enable();
                                return;
                            }

                            Ext.getCmp('chkStatusWarning').show();
                        },
                        focus: function (field, event, eOpts) {

                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 100);
                            }
                        }
                    }
                },
                {

                    xtype: 'radiogroup',
                    fieldLabel: '排序',
                    name: 'GRID_SORT',
                    id: 'grid_sort',
                    hidden: true,
                    columns: 2,
                    items: [
                        { boxLabel: '<span style="font-size:20px">院內碼</span>', name: 'ST_TYPE', inputValue: 1 },
                        { boxLabel: '<span style="font-size:20px">儲位</span>', name: 'ST_TYPE', inputValue: 2, checked: true }
                    ]

                },
                {
                    xtype: 'box',
                    width: '100%',
                    id: 'chkStatusWarning',
                    hidden: true,
                    renderTpl: [
                        '<table width="100%"><tr><td align="center">'
                        + '<span style="font-size:1.5em;">已完成盤點，不可輸入</span>'
                        + '</td></tr></table>'
                    ],
                    width: '100%'
                }, {
                    xtype: 'button',
                    text: '開始盤點',
                    id: 'btnStart',
                    //width: '15%',
                    disabled: true,
                    margin: '8 0 0 8',
                    handler: function () {
                        //mask.show();
                        var info = '';
                        //info += chk_period == 'D' ? T1Query.getForm().findField('CHK_YM_DATE').getValue() : T1Query.getForm().findField('CHK_YM_MONTH').getValue();
                        //info += ' ';
                        info += T1Query.getForm().findField('CHK_NO').rawValue;
                        T21Query.getForm().findField('CHK_MAST_DETAIL').setValue(info);

                        if (T1Query.getForm().findField('GRID_SORT').getValue()['ST_TYPE'] == "1") {
                            sort_by = 'mmcode';
                        } else if (T1Query.getForm().findField('GRID_SORT').getValue()['ST_TYPE'] == "2") {
                            sort_by = 'store_loc';
                        }
                        sort_order = 'asc';

                        Ext.getCmp('btn_confirm').disable();
                        //var chkTypeString = '';
                        //if (chk_level == '1') { chkTypeString += '初盤 ' }
                        //if (chk_level == '2') { chkTypeString += '複盤 ' }
                        //if (chk_level == '3') { chkTypeString += '三盤 ' }
                        //chkTypeString += T1Query.getForm().findField('CHK_NO').rawValue;
                        //itemDetail.getForm().findField('itemDetailChkType').setValue(chkTypeString);
                        //itemDetail.getForm().findField('CHK_QTY').setValue('');
                        //itemDetail.getForm().findField('STORE_LOC').setValue('');
                        Ext.getCmp('CHK_UID_COMBO').hide();
                        Ext.getCmp('btnChkuidShow').hide();

                        if (chk_uid_count == 0) {
                            Ext.getCmp('btnChkuidShow').show();
                            setChkuidComboData();
                            Ext.getCmp('t1Card').setHeight(windowHeight - 105);
                        } else {
                            Ext.getCmp('t1Card').setHeight(windowHeight - 105);
                        }

                        //T12Store.setSorters()

                        getFirstItem();
                        //T11Load();
                        //T12Load();
                        changeCardT10();
                        changeCardItemDetail();
                        setTimeout(function () {

                            itemDetail.getForm().findField('CHK_QTY').disable();
                            T21Query.getForm().findField('BARCODE').setValue('');
                            T21Query.getForm().findField('BARCODE').focus();
                        }, 300);
                        //T1Load();
                    }
                }
            ]
        }
        ]
    });

    function getDefaultMonthValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
    }

    function getDefaultDateValue() {
        var yyyy = 0;
        var m = 0;
        var d = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        d = new Date().getDate();
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        var dd = d >= 10 ? d.toString() : "0" + d.toString();
        return yyyy.toString() + mm + dd;
    }

    function updateMsgCount() {
        Ext.Ajax.request({
            url: msgCountGet,
            method: reqVal_p,
            params:
            {
                chk_no: T1Query.getForm().findField('CHK_NO').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;

                    if (tb_data.length > 0) {
                        msglabel('應盤：' + tb_data[0].CHK_TOTAL + ' 未盤：' + tb_data[0].NOSIGN);

                        Ext.getCmp('btnDoneList').disable();
                        Ext.getCmp('btnDoneList').disable();
                        Ext.getCmp('btn_pass').disable();
                        Ext.getCmp('btnAllDone').disable();
                        if (tb_data[0].NOSIGN == "0") {
                            Ext.Msg.alert('訊息', '查無未盤的品項');
                            Ext.getCmp('btnDoneList').enable();
                            changeCardT11();
                            Ext.getCmp('btnAllDone').enable();
                        }
                        else if (tb_data[0].CHK_TOTAL == tb_data[0].NOSIGN) {
                            Ext.getCmp('btnUndoneList').enable();
                            Ext.getCmp('btn_pass').enable();
                        } else {
                            Ext.getCmp('btnUndoneList').enable();
                            Ext.getCmp('btnDoneList').enable();
                            Ext.getCmp('btn_pass').enable();
                            Ext.getCmp('btnAllDone').enable();
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });

    }

    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            T11Query.getForm().findField('BARCODE').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
    });

    var T21Query = Ext.widget({
        itemId: 'queryformT21',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelWidth: 90,
            width: '100%'
        },

        items: [{
            xtype: 'container',
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'hbox',
                        //vertical: true,
                        //width:'100%'
                    },
                    items: [
                        {
                            xtype: 'button',
                            text: '<span style="font-size:16px">選擇盤點單</span>',
                            id: 'btnBackQuery',
                            //width: '15%',
                            margin: '0 0 0 8',
                            handler: function () {
                                T1Query.getForm().findField('CHK_NO').setValue('');
                                Ext.getCmp('grid_sort').hide();
                                setChknoComboData();

                                changeCardQuery();
                            }

                        },
                        {
                            xtype: 'container',
                            right: '100%',
                            hidden: true,
                            items: [
                                {
                                    xtype: 'displayfield',
                                    itemId: 'CHK_MAST_DETAIL',
                                    name: 'CHK_MAST_DETAIL',
                                    label: false,
                                    fieldStyle: {
                                        'font-size': '14px!important'
                                    },
                                }
                            ]
                        },
                        {
                            xtype: 'component',
                            flex: 1
                        },
                        {
                            xtype: 'button',
                            text: '<font size="2vmin">○</font>',
                            width: '7%',
                            id: 'btnChkuidShow',
                            margin: '0 0 0 8',
                            hidden: true,
                            handler: function () {

                                var f = T21Query.getForm();
                                if (Ext.getCmp('CHK_UID_COMBO').hidden) {
                                    Ext.getCmp('CHK_UID_COMBO').show();
                                    this.setText('<font size="2vmin">●</font>');
                                    Ext.getCmp('t1Card').setHeight(windowHeight - 135);
                                }
                                else {
                                    Ext.getCmp('CHK_UID_COMBO').hide();
                                    this.setText('<font size="2vmin">○</font>');
                                    Ext.getCmp('t1Card').setHeight(windowHeight - 105);
                                }
                            }
                        },


                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    hidden: true,
                    id: 'CHK_UID_COMBO',
                    layout: {
                        type: 'hbox',
                        vertical: true,
                        width: '100%'
                    },
                    items: [
                        {
                            xtype: 'combo',
                            store: st_people,
                            fieldLabel: '盤點人員',
                            name: 'CHK_UID',
                            id: 'CHK_UID',
                            width: '100%',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            autoSelect: true,
                            anyMatch: true,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            listeners: {
                                select: function (c, r, i, e) {
                                    T11Load();
                                    T12Load();
                                },
                                focus: function (field, event, eOpts) {
                                    if (!field.isExpanded) {
                                        setTimeout(function () {
                                            field.expand();
                                        }, 300);
                                    }
                                }
                            }
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '條碼',
                            name: 'BARCODE',
                            id: 'BARCODE',
                            width: '90%',
                            labelWidth: false,
                            labelStyle: 'width: 28%',
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length > 7) {
                                        chkBarcode(nValue);
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            iconCls: 'TRACamera',
                            width: '9%',
                            handler: function () {
                                showScanWin(viewport);
                            }
                        }
                    ]
                },

            ]
        }
        ]
    });

    var T1Foot = Ext.widget({
        itemId: 'footform',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 25%',
            width: '35%'
        },

        items: [{
            xtype: 'container',
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'hbox',
                    },
                    items: [{
                        xtype: 'button',
                        id: 'btnDoneList',
                        text: '已盤清單',
                        width: '33%',
                        margin: '0 8 0 0',
                        disabled: true,
                        handler: function () {
                            changeCardT11();
                            showGrid = 'done';
                            T21Query.getForm().findField('BARCODE').setValue('');
                            T11Load();
                        }
                    }, {
                        xtype: 'button',
                        id: 'btnUndoneList',
                        text: '未盤清單',
                        width: '33%',
                        margin: '0 8 0 0',
                        disabled: true,
                        handler: function () {
                            changeCardT12();
                            showGrid = 'undone';
                            T21Query.getForm().findField('BARCODE').setValue('');
                            T12Load();
                        }
                    }, {
                        xtype: 'button',
                        id: 'btnAllDone',
                        text: '完成盤點',
                        width: '33%',
                        margin: '0 0 0 0',
                        disabled: true,
                        handler: setFinalData
                    }]
                }
            ]
        }
        ]
    });

    // 同品項不同儲位
    var T10Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T11Model',
        pageSize: 2000,
        //remoteSort: true,
        //sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {

                mask.show();
                var r = Ext.create('WEBAPP.model.CE0003');
                itemDetail.loadRecord(r);
                var np = {
                    p0: T21Query.getForm().findField('BARCODE').getValue(),
                    p1: T1Query.getForm().findField('CHK_NO').getValue(),
                    p2: chk_uid_count == 0 ? T21Query.getForm().findField('CHK_UID').getValue() : '',
                    p3: T1Query.getForm().findField('GRID_SORT').getValue()['ST_TYPE']
                };
                Ext.apply(store.proxy.extraParams, np);

            },
            load: function (store, records, successful, operation, eOpts) {

                if (mask)
                    mask.hide();
                if (records.length > 0) {

                    if (records.length == 1) {
                        itemDetail.loadRecord(records[0]);
                        if (records[0].data['STATUS_INI'] == '1') {
                            //Ext.getCmp('btn_pass').setDisabled(false);
                            //Ext.getCmp('btn_confirm').setDisabled(false);

                            Ext.getCmp('btn_confirm').enable();
                            itemDetail.getForm().findField('CHK_QTY').enable();

                            // 藥庫預設盤點量為電腦量
                            if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S')
                                itemDetail.getForm().findField('CHK_QTY').setValue(records[0].data['STORE_QTYC']);

                            changeCardT10();
                            setTimeout(function () {
                                //itemDetail.getForm().findField('CHK_QTY').focus();
                                if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S') {
                                    itemDetail.getForm().findField('STORE_LOC').focus();
                                } else {
                                    itemDetail.getForm().findField('CHK_QTY').focus();
                                }

                            }, 300);
                        }
                        else {
                            Ext.Msg.alert('提醒', '此項目已完成盤點', function () {
                                itemDetail.getForm().findField('CHK_QTY').disable();
                                Ext.getCmp('btn_pass').setDisabled(true);
                                Ext.getCmp('btn_confirm').setDisabled(true);
                            });
                        }


                    }
                    else {
                        // 切換到同品項不同儲位清單
                        var layoutG = viewport.down('#t1Card').getLayout();
                        layoutG.setActiveItem(3);
                    }
                }
                else {
                    Ext.Msg.alert('訊息', '此盤點單查無該品項', function () {
                        T21Query.getForm().findField('BARCODE').setValue('');
                    });

                }

            }
        },
        proxy: {
            type: 'ajax',
            //timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0003/GetChkBarcode',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    var T11Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T11Model',
        pageSize: 9999,
        //remoteSort: true,

        //sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                mask.show();
                var f = T1Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    d0: chk_ym,
                    wh_no: f.findField('WH_NO').getValue(),
                    chk_no: f.findField('CHK_NO').getValue(),
                    barcode: T21Query.getForm().findField('BARCODE').getValue(),
                    grid_sort: f.findField('GRID_SORT').getValue()['ST_TYPE'],
                    ischk: 'Y',
                    sort_order: sort_order,
                    sort_by: sort_by,
                    chk_uid: T21Query.getForm().findField('CHK_UID').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

            },
            load: function (store, records, successful, operation, eOpts) {
                // Ext.getCmp('btnDoneList').setDisabled(records.length == 0);
                //Ext.getCmp('btnAllDone').setDisabled(records.length == 0);
                updateMsgCount();
                mask.hide();
            }

        },
        proxy: {
            type: 'ajax',
            //timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0003/GetChk',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    var T12Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T11Model',
        pageSize: 9999,
        //remoteSort: true,

        //sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforesort: function (store, sorters, eOpts) {

                if (sorters.length == 0) {
                    return;
                }
                //var sorter = sorters[0].config;
                sort_order = sorters[0]._direction;
                sort_by = sorters[0]._property;
            },
            beforeload: function (store, options) {
                mask.show();
                var f = T1Query.getForm();

                // 載入前將查詢條件代入參數
                var np = {
                    d0: chk_ym,
                    wh_no: f.findField('WH_NO').getValue(),
                    chk_no: f.findField('CHK_NO').getValue(),
                    barcode: T21Query.getForm().findField('BARCODE').getValue(),
                    //grid_sort: f.findField('GRID_SORT').getValue()['ST_TYPE'],
                    sort_order: sort_order,
                    sort_by: sort_by,
                    ischk: 'N',
                    chk_uid: T21Query.getForm().findField('CHK_UID').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

            },
            load: function (store, records, successful, operation, eOpts) {
                mask.hide();
                if (records.length > 0) {
                    changeCardT12();
                }
                else {
                    Ext.Msg.alert('訊息', '查無未盤的品項');
                    changeCardT11();
                }

                Ext.getCmp('btnUndoneList').setDisabled(records.length == 0);
                updateMsgCount();

            }
        },
        proxy: {
            type: 'ajax',
            //timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0003/GetChk',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    function T11Load() {
        T11Tool.moveFirst();
        msglabel('訊息區:');
    }

    function T12Load() {
        T12Tool.moveFirst();
        msglabel('訊息區:');
    }

    function T10Load() {
        T10Tool.moveFirst();
    }

    var T10Tool = Ext.create('Ext.PagingToolbar', {
        store: T10Store,
        displayInfo: true,
        border: false,
        plain: true,
    });
    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
    });
    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
        displayInfo: true,
        border: false,
        plain: true,
    });

    var T10Grid = Ext.create('Ext.grid.Panel', {
        store: T10Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: {
            items: [{
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '20%',
                sortable: true
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: '20%',
                sortable: true
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: '15%',
                sortable: true
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '20%',
                sortable: true
            }, {
                text: "商品條碼",
                dataIndex: 'BARCODE',
                width: '25%',
                sortable: true
            }, {
                text: "庫存量",
                dataIndex: 'STORE_QTYC',
                width: '85px',
                sortable: true
            },]
        },
        viewConfig: {
            //stripeRows: true,
            //getRowClass: function (record, rowIndex, rowParams, store) {
            //    return 'multiline-row';
            //},
            listeners: {
                selectionchange: function (model, records) {
                    T10LastRec = records[0];
                },
                cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {

                    T10LastRec = record;
                    itemDetail.loadRecord(T10LastRec);
                    // 藥庫預設盤點量為電腦量
                    if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S')
                        itemDetail.getForm().findField('CHK_QTY').setValue(record.data['STORE_QTYC']);

                    Ext.getCmp('btn_pass').setDisabled(true);

                    if (T10LastRec.data['STATUS_INI'] == '1') {
                        Ext.getCmp('btn_confirm').setDisabled(false);
                        itemDetail.getForm().findField('CHK_QTY').enable();
                    }
                    else {
                        Ext.getCmp('btn_confirm').setDisabled(true);
                        itemDetail.getForm().findField('CHK_QTY').disable();
                    }

                    // 切換到明細Form
                    var layoutQ = viewport.down('#t1Query').getLayout();
                    layoutQ.setActiveItem(0);
                    var layoutG = viewport.down('#t1Card').getLayout();
                    layoutG.setActiveItem(0);
                    setTimeout(function () {
                        if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S') {
                            itemDetail.getForm().findField('STORE_LOC').focus();
                        }
                        else {
                            itemDetail.getForm().findField('CHK_QTY').focus();
                        }
                    }, 300);
                }
            }
        }
    });

    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: {
            items: [{
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '32%',
                sortable: true
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: '34%',
                sortable: true
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '34%',
                sortable: true
            }, {
                text: "庫存量",
                dataIndex: 'STORE_QTYC',
                width: '85px',
                sortable: true
            },]
        },
        viewConfig: {
            //stripeRows: true,
            //getRowClass: function (record, rowIndex, rowParams, store) {
            //    return 'multiline-row';
            //},
            listeners: {
                selectionchange: function (model, records) {
                    T11LastRec = records[0];
                },
                cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {

                    T11LastRec = record;
                    itemDetail.loadRecord(T11LastRec);
                    if (T11LastRec.data['STATUS_INI'] == '1' && (T11LastRec.data['CHK_TIME'] == null || T11LastRec.data['CHK_TIME'] == '')) {
                        Ext.getCmp('btn_pass').setDisabled(false);
                    }
                    else {
                        Ext.getCmp('btn_pass').setDisabled(true);
                    }
                    if (T11LastRec.data['STATUS_INI'] == '1') {
                        itemDetail.getForm().findField('CHK_QTY').enable();
                        Ext.getCmp('btn_confirm').setDisabled(false);

                    }
                    else
                        Ext.getCmp('btn_confirm').setDisabled(true);
                    // itemDetail.getForm().findField('CHK_QTY').setReadOnly(true);
                    // 切換到明細Form
                    var layoutQ = viewport.down('#t1Query').getLayout();
                    layoutQ.setActiveItem(0);
                    var layoutG = viewport.down('#t1Card').getLayout();
                    layoutG.setActiveItem(0);
                    setTimeout(function () {
                        itemDetail.getForm().findField('CHK_QTY').disable();
                        T21Query.getForm().findField('BARCODE').focus();
                        Ext.getCmp('btn_confirm').disable();

                        if (itemDetail.getForm().findField('STORE_QTYC').getValue() == '0') {
                            itemDetail.getForm().findField('CHK_QTY').enable();
                            itemDetail.getForm().findField('STORE_LOC').focus();
                            Ext.getCmp('btn_confirm').enable();
                        }

                    }, 300);
                }
            }
        }
    });

    var T12Grid = Ext.create('Ext.grid.Panel', {
        store: T12Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: {
            items: [{
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '32%',
                sortable: true
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: '34%',
                sortable: true
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '34%',
                sortable: true
            }, {
                text: "庫存量",
                dataIndex: 'STORE_QTYC',
                width: '85px',
                sortable: true
            },]
        },
        viewConfig: {
            //stripeRows: true,
            //getRowClass: function (record, rowIndex, rowParams, store) {
            //    return 'multiline-row';
            //},
            listeners: {
                selectionchange: function (model, records) {
                    T12LastRec = records[0];
                },
                cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {

                    T12LastRec = record;
                    itemDetail.loadRecord(T12LastRec);

                    //if (T12LastRec.data['STATUS_INI'] == '1' && (T12LastRec.data['CHK_TIME'] == null || T12LastRec.data['CHK_TIME'] == '')) {
                    //    Ext.getCmp('btn_pass').setDisabled(false);
                    //    Ext.getCmp('btn_confirm').setDisabled(false);
                    //}
                    //else {
                    //    Ext.getCmp('btn_pass').setDisabled(true);
                    //    Ext.getCmp('btn_confirm').setDisabled(true);
                    //}
                    //if (T12LastRec.data['STATUS_INI'] == '1') {
                    //    itemDetail.getForm().findField('CHK_QTY').enable();
                    //    Ext.getCmp('btn_confirm').setDisabled(false); 

                    //}
                    //else
                    //    Ext.getCmp('btn_confirm').setDisabled(true);

                    // 藥庫預設盤點量為電腦量
                    if (T1Query.getForm().findField('WH_NO').getValue() == 'PH1S')
                        itemDetail.getForm().findField('CHK_QTY').setValue(T12LastRec.data['STORE_QTYC']);

                    itemDetail.getForm().findField('CHK_QTY').enable();
                    // 切換到明細Form
                    var layoutQ = viewport.down('#t1Query').getLayout();
                    layoutQ.setActiveItem(0);
                    var layoutG = viewport.down('#t1Card').getLayout();
                    layoutG.setActiveItem(0);
                    setTimeout(function () {
                        itemDetail.getForm().findField('CHK_QTY').disable();
                        T21Query.getForm().findField('BARCODE').focus();
                        Ext.getCmp('btn_confirm').disable();

                        if (itemDetail.getForm().findField('STORE_QTYC').getValue() == '0') {
                            itemDetail.getForm().findField('CHK_QTY').enable();
                            itemDetail.getForm().findField('STORE_LOC').focus();
                            Ext.getCmp('btn_confirm').enable();
                        }
                    }, 300);
                }
            }
        }
    });

    var itemDetail = Ext.create('Ext.form.Panel', {
        id: 'itemDetail',
        height: '100%',
        layout: 'fit',
        closable: false,
        border: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 100,
            width: '100%'
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                padding: '2vmin',
                scrollable: true,
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '電腦量',
                        name: 'STORE_QTYC',
                        readOnly: true
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '盤點量',
                        name: 'CHK_QTY',
                        maskRe: /[0-9]/,
                        fieldCls: 'required',
                        allowBlank: false,
                        readOnly: false,
                        disabled: true,
                        selectOnFocus: true,
                        enableKeyEvents: true,
                        listeners: {
                            keydown: function (field, e, eOpts) {
                                if (e.keyCode == '13' && Ext.getCmp('btn_confirm').disabled == false) {
                                    Ext.getCmp('btn_confirm').click();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '儲位',
                        name: 'STORE_LOC',
                        submitValue: true,
                        enableKeyEvents: true,
                        listeners: {
                            keydown: function (field, e, eOpts) {

                                if (e.keyCode == '13' && Ext.getCmp('btn_confirm').disabled == false) {
                                    itemDetail.getForm().findField('CHK_QTY').focus();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        readOnly: true,
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '計量單位',
                        name: 'BASE_UNIT',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '盤點時間',
                        name: 'CHK_TIME',
                        readOnly: true,
                        renderer: function (value, meta, record) {
                            if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (Taipei Standard Time)") {  //value值是 null時候
                                return '';
                            } else if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (台北標準時間)") {
                                return '';
                            } else {
                                return Ext.util.Format.date(value, 'Xmd H:i:s');
                            }
                        },
                    }, {
                        xtype: 'hidden',
                        name: 'CHK_NO',
                        submitValue: true
                    }
                ]
            }
        ],
        buttons: [{
            id: 'btn_pass',
            disabled: false,
            text: '<font size="4vmin">略過</font>',
            height: '8vmin',
            width: '29%',
            disabled: true,
            handler: function () {
                getFirstItem(itemDetail.getForm().findField('MMCODE').getValue(), itemDetail.getForm().findField('STORE_LOC').getValue());
            }
        }, {
            id: 'btn_confirm',
            disabled: false,
            text: '<font size="4vmin">確定</font>',
            height: '8vmin',
            width: '70%',
            disabled: true,
            handler: function () {
                if (parseInt(this.up('form').getForm().findField('CHK_QTY').getValue()) >= 0) {
                    if (this.up('form').getForm().findField('CHK_QTY').getValue() != "") {

                        if (Number(this.up('form').getForm().findField('CHK_QTY').getValue()) ==
                            Number(this.up('form').getForm().findField('STORE_QTYC').getValue())) {

                            T1Submit(showGrid);
                            return;
                        }

                        var msg = '電腦量：' + this.up('form').getForm().findField('STORE_QTYC').getValue() + '<br>';
                        msg += '盤點量：' + this.up('form').getForm().findField('CHK_QTY').getValue() + '<br>';
                        msg += '兩者不同，是否儲存？';

                        Ext.MessageBox.confirm('提示', msg, function (btn, text) {
                            if (btn === 'yes') {
                                //transferToBcwhpick("");
                                T1Submit(showGrid);
                            }
                        }
                        );


                    } else {
                        msglabel('盤點量不可空白，不能送出!');
                    }
                } else {
                    msglabel('盤點量不可空白或小於0!');
                }
            }
        }]
    });

    //完成盤點
    function setFinalData() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: FinalPro,
            method: reqVal_p,
            params: { chk_no: T1Query.getForm().findField('CHK_NO').getValue() },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_msg = data.msg;
                    var tb_etts = data.etts;
                    var tips = "";

                    //Ext.ComponentQuery.query('panel[itemId=queryform2]')[0].collapse();

                    if (tb_msg == "尚有負責的品項未盤點!") {  //尚有負責的品項未盤點
                        for (var i = 0; i < tb_etts.length; i++) {
                            tips = tips + "院內碼: " + tb_etts[i].MMCODE + ", 儲位: " + tb_etts[i].STORE_LOC + "<br>";
                        }
                        tips = tips + "品項未盤點";

                        Ext.Msg.alert('提醒', tips);
                    } else if (tb_msg == "您已經完成此單號的盤點!") {   //做update
                        Ext.Msg.alert('提醒', data.msg, function () {
                            T11Load();
                            T12Load();
                            msglabel(tb_msg);
                        });
                    }

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    function T1Submit(showGrid) {
        
        var f = itemDetail.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Update,
                success: function (form, action) {
                    myMask.hide();
                    itemDetail.getForm().findField('CHK_QTY').disable();
                    T21Query.getForm().findField('BARCODE').setValue('');
                    T21Query.getForm().findField('BARCODE').focus();

                    if (showGrid != 'done') {
                        getFirstItem(itemDetail.getForm().findField('MMCODE').getValue(), itemDetail.getForm().findField('STORE_LOC').getValue())
                    } else {
                        T11Load();
                        changeCardT11();
                    }
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    // 檢查院內碼
    function chkBarcode(barcode) {
        T10Load();
    }

    function changeCardT10() {
        setTimeout(function () {
            Ext.getCmp('btnDoneList').btnInnerEl.setStyle({ 'color': 'black', 'font-weight': 'bold' });
            Ext.getCmp('btnUndoneList').btnInnerEl.setStyle({ 'color': 'black', 'font-weight': 'bold' });
            var layoutG = viewport.down('#t1Card').getLayout();
            layoutG.setActiveItem(0);
        }, 100);
    }

    function changeCardT11() {
        setTimeout(function () {
            Ext.getCmp('btnDoneList').btnInnerEl.setStyle({ 'color': 'red', 'font-weight': 'bold' });
            Ext.getCmp('btnUndoneList').btnInnerEl.setStyle({ 'color': 'black', 'font-weight': 'bold' });
            var layoutG = viewport.down('#t1Card').getLayout();
            layoutG.setActiveItem(1);
        }, 100);
    }

    function changeCardT12() {
        setTimeout(function () {
            Ext.getCmp('btnDoneList').btnInnerEl.setStyle({ 'color': 'black', 'font-weight': 'bold' });
            Ext.getCmp('btnUndoneList').btnInnerEl.setStyle({ 'color': 'red', 'font-weight': 'bold' });
            var layout = viewport.down('#t1Card').getLayout();
            layout.setActiveItem(2);
        }, 100);
    }

    function changeCardQuery() {
        setTimeout(function () {
            var layoutG = viewport.down('#outerCard').getLayout();
            layoutG.setActiveItem(0);
            Ext.getCmp('t1Foot').hide();
        }, 100);
    }

    function changeCardItemDetail() {
        setTimeout(function () {
            var layout = viewport.down('#outerCard').getLayout();
            layout.setActiveItem(1);
            Ext.getCmp('t1Foot').show();
        }, 100);
    }

    var chkqtyInput = Ext.widget({
        itemId: 'chkqtyInput',
        xtype: 'container',
        border: false,
        autoScroll: true,
        width: '100%',
        //collapsible: true,
        //hideCollapseTool: true,
        //titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 90,
            //labelStyle: 'width: 25%',
            width: '100%',
            labelStyle: "font-size:20px;font-weight:bold;",
            fieldStyle: "font-size:20px",
        },
        items: [
            {
                itemId: 't1Query',
                region: 'north',
                layout: 'card',
                split: true,
                //collapsible: true,
                border: false,
                hideCollapseTool: true,
                items: [T21Query]
            }, {
                id: 't1Card',
                region: 'center',
                layout: 'card',
                collapsible: false,
                title: '',
                height: windowHeight - 105,
                border: false,
                items: [itemDetail, T11Grid, T12Grid, T10Grid]
            },
            {
                itemId: 't1Foot',
                region: 'south',
                layout: 'fit',
                id: 't1Foot',
                split: false,
                hidden: true,
                //collapsible: true,
                border: false,
                items: [T1Foot]
            }
        ]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            id: 'outerCard',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            split: false,
            //items: [T1Query, itemDetail]
            items: [T1Query, chkqtyInput]
        },
        ]
    });
    var mask = new Ext.LoadMask(viewport, { msg: '處理中...' });

    setWhnoComboData();

    T1Query.getForm().findField('GRID_SORT').setValue({ ST_TYPE: 2 });
    //#endregion
});
