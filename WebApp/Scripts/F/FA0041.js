Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Name = "有盤點為盤點單位查詢(含藥品)";
    var unitClassGet = '../../../api/FA0041/GetUnitClassConbo';
    var T1GetExcel = '../../../api/FA0041/Excel';
    var T2GetExcel = '../../../api/FA0041/DoneExcel';
    var T3GetExcel = '../../../api/FA0041/UndoneExcel';
    var reportUrl = '/Report/F/FA0041.aspx';

    var p2 = 'Y';   // 有盤點單位:Y 無盤點單位:N (defailt: Y)
    var viewModel = Ext.create('WEBAPP.store.FA0041VM');
    // 單位分類清單
    var unitClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 盤點類別清單
    var chkTypeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 盤點狀態清單
    var chkStatusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    function showReport(record) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?CHK_YM=' + T1Query.getForm().findField('P0').rawValue
                + '&UNIT_CLASS=' + T1Query.getForm().findField('P1').getValue()
                + '&UNIT_CLASS_NAME=' + T1Query.getForm().findField('P1').rawValue
                + '&IS_CHK=' + p2
                + '&CHK_TYPE=' + T1Query.getForm().findField('P3').getValue()
                + '&CHK_STATUS=' + T1Query.getForm().findField('P4').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }
    function dateTransform(date) {
        var yyyy = date.getFullYear().toString();
        var m = (date.getMonth() + 1).toString();
        var d = date.getDate().toString();
        var mm = m.length < 2 ? "0" + m : m;
        var dd = d.length < 2 ? "0" + d : d;
        return yyyy + "-" + mm + "-" + dd;
    }

    function getUnitclassCombo() {
        Ext.Ajax.request({
            url: unitClassGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var unitClasses = data.etts;
                    if (unitClasses.length > 0) {
                        for (var i = 0; i < unitClasses.length; i++) {
                            unitClassStore.add({ VALUE: unitClasses[i].VALUE, TEXT: unitClasses[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getChkTypeCombo() {
        Ext.Ajax.request({
            url: '/api/FA0041/GetChkTypeCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var chkTypes = data.etts;
                    if (chkTypes.length > 0) {
                        chkTypeStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < chkTypes.length; i++) {

                            chkTypeStore.add({ VALUE: chkTypes[i].VALUE, TEXT: chkTypes[i].TEXT });
                        }
                    }
                    T1Query.getForm().findField('P3').setValue('');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getChkStatusCombo() {
        Ext.Ajax.request({
            url: '/api/FA0041/GetChkStatusCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var chkStatus = data.etts;
                    if (chkStatus.length > 0) {
                        chkStatusStore.add({ VALUE: '', TEXT: '全部' });
                        T2_chkStatusStore.add({ VALUE: '', TEXT: '全部' });
                        T3_chkStatusStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < chkStatus.length; i++) {
                            chkStatusStore.add({ VALUE: chkStatus[i].VALUE, TEXT: chkStatus[i].TEXT });
                            if (chkStatus[i].IS_DONE == 'Y') {
                                T2_chkStatusStore.add({ VALUE: chkStatus[i].VALUE, TEXT: chkStatus[i].TEXT });
                            }
                            if (chkStatus[i].IS_DONE == 'N') {
                                T3_chkStatusStore.add({ VALUE: chkStatus[i].VALUE, TEXT: chkStatus[i].TEXT });
                            }
                        }
                    }
                    T1Query.getForm().findField('P4').setValue('');
                    T2Query.getForm().findField('P3').setValue('');
                    T2Query.getForm().findField('P4').setValue('');
                    T3Query.getForm().findField('P3').setValue('');
                    T3Query.getForm().findField('P4').setValue('');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var getFileNameDate = function () {
        var d1 = T1Query.getForm().findField('P0').getValue();
        return getYYYMMDateString(d1);
    }
    function getYYYMMDateString(date) {
        var yyy = date.getFullYear() - 1911;
        var m = (date.getMonth() + 1).toString();
        var mm = m.length < 2 ? "0" + m : m;
        return yyy.toString() + mm;
    }
    var getDateString = function (date) {
        var y = (date.getFullYear()).toString();
        var m = (date.getMonth() + 1).toString();
        var d = (date.getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + "-" + m + "-" + d;
    }
    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }


    //#region T1Grid
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [

                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    value: new Date(),
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    store: unitClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '單位類別',
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P1',
                    allowBlank: false,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 200,
                    labelWidth: 62,
                    fieldCls: 'required',
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'radiogroup',
                    name: 'P2',
                    fieldLabel: '',
                    padding: '0 4 0 4',
                    items: [
                        { boxLabel: '有開單單位', name: 'P2', inputValue: 'Y', width: 80, checked: true },
                        { boxLabel: '未開單單位', name: 'P2', inputValue: 'N', width: 80 }
                    ],
                    //labelWidth: 100,
                    width: 200,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            changeP2(newValue['P2']);
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: chkTypeStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點類別',
                    queryMode: 'local',
                    name: 'P3',
                    id: 'P3',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: chkStatusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點狀態',
                    queryMode: 'local',
                    name: 'P4',
                    id: 'P4',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load(true);

                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        }
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                    }
                }
            ]
        },
        ]
    });

    function changeP2(newP2) {
        p2 = newP2;
        if (p2 == 'N') {
            T1Query.getForm().findField('P4').disable();
        } else {
            T1Query.getForm().findField('P4').enable();
        }
    }

    var T1Store = viewModel.getStore('All');
    function T1Load(clearMsg) {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", p2);
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());

        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('');
        }
    }
    T1Store.on('load', function (store, options) {
        
        if (store.totalCount > 0) {
            Ext.getCmp('print').enable();
            Ext.getCmp('export').enable();
        } else {
            Ext.getCmp('print').disable();
            Ext.getCmp('export').disable();
        }
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'print',
                text: '列印',
                handler: function () {
                    if (T1Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                        return;
                    }
                    showReport();
                },
                disabled:true
            },
            {
                id: 'export',
                text: '匯出',
                handler: function () {
                    
                    if (T1Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        return;
                    }
                    var fn = T1Query.getForm().findField('P0').rawValue + '_' + (p2 == 'Y' ? '有盤點單位' : '未盤點單位') + '.xls';

                    var p = new Array();
                    p.push({ name: 'FN', value: fn });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: p2 });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });

                    PostForm(T1GetExcel, p);
                },
                disabled: true
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'panel',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 150,

        }, {
            text: "盤點類別",
            dataIndex: 'CHK_TYPE',
            width: 120,
        }, {
            text: "盤點期",
            dataIndex: 'CHK_PERIOD',
            width: 80,
        }, {
            text: "初盤單號",
            dataIndex: 'CHK_NO',
            width: 100,
        }, {
            text: "盤點年月",
            dataIndex: 'CHK_YM',
            width: 100,
        }, {
            text: "最後盤點階段",
            dataIndex: 'CHK_LEVEL',
            width: 100,
        }, {
            text: "盤點單狀態",
            dataIndex: 'CHK_STATUS',
            width: 100,
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_YM');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }
                
            },
        }
    });
    //#endregion

    //#region T2Grid
    // 盤點狀態清單
    var T2_chkStatusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var whno_Combo_2 = Ext.create('WEBAPP.form.WH_NoCombo', {
        //name: 'WH_NO',
        name: 'P5',
        id: 'P2P5',
        fieldLabel: '庫房代碼',
        width: 230,
        labelWidth: 70,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0041/GetWhnoCombo',

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                if (r.get('WH_NO') !== '') {
                    T2Query.getForm().findField('P2P5').setValue(r.get('WH_NO'));
                }
                
            }
        }
    });
    var mmCodeCombo_2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P6',
        id:'P2P6',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 230,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0041/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                if (r.get('MMCODE') !== '') {
                    T2Query.getForm().findField('P2P6').setValue(r.get('MMCODE'));
                }
            }
        }
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [

                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P2P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    value: new Date(),
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    store: unitClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '單位類別',
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P2P1',
                    allowBlank: false,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 200,
                    labelWidth: 62,
                    fieldCls: 'required',
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: chkTypeStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點類別',
                    queryMode: 'local',
                    name: 'P3',
                    id: 'P2P3',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: ' ',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: T2_chkStatusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點狀態',
                    queryMode: 'local',
                    name: 'P4',
                    id: 'P2P4',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: ' ',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T2Query.getForm().isValid()) {
                            T2Load(true);

                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        }
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP21',
            border: false,
            layout: 'hbox',
            items: [
                whno_Combo_2,
                mmCodeCombo_2,
                {
                    xtype: 'textfield',
                    fieldLabel: '初盤單號',
                    name: 'P7',
                    id: 'P2P7',
                    allowBlank: true,
                    maxLength: 30,          // 可輸入最大長度為100
                },
            ]
        },
        ]
    });
    var T2Store = viewModel.getStore('DoneDatas');
    function T2Load(clearMsg) {
        T2Store.getProxy().setExtraParam("p0", T2Query.getForm().findField('P0').rawValue);
        T2Store.getProxy().setExtraParam("p1", T2Query.getForm().findField('P1').getValue());
        T2Store.getProxy().setExtraParam("p3", T2Query.getForm().findField('P3').getValue());
        T2Store.getProxy().setExtraParam("p4", T2Query.getForm().findField('P4').getValue());
        T2Store.getProxy().setExtraParam("p5", T2Query.getForm().findField('P5').getValue());
        T2Store.getProxy().setExtraParam("p6", T2Query.getForm().findField('P6').getValue());
        T2Store.getProxy().setExtraParam("p7", T2Query.getForm().findField('P7').getValue());

        T2Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    T2Store.on('load', function (store, options) {

        if (store.totalCount > 0) {
            Ext.getCmp('export_T2').enable();
        } else {
            Ext.getCmp('export_T2').disable();
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'export_T2',
                text: '匯出',
                handler: function () {

                    if (T2Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        return;
                    }
                    var fn = T2Query.getForm().findField('P0').rawValue + '_已完成盤點明細.xls';

                    var p = new Array();
                    p.push({ name: 'FN', value: fn });
                    p.push({ name: 'p0', value: T2Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T2Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p3', value: T2Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T2Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T2Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T2Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T2Query.getForm().findField('P7').getValue() });

                    PostForm(T2GetExcel, p);
                },
                disabled: true
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'panel',
                items: [T2Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 150,

        }, {
            text: "盤點類別",
            dataIndex: 'CHK_TYPE',
            width: 120,
        }, {
            text: "盤點期",
            dataIndex: 'CHK_PERIOD',
            width: 80,
        }, {
            text: "初盤單號",
            dataIndex: 'CHK_NO',
            width: 100,
        }, {
            text: "盤點年月",
            dataIndex: 'CHK_YM',
            width: 100,
        }, {
            text: "最後盤點階段",
            dataIndex: 'CHK_LEVEL',
            width: 100,
        }, {
            text: "盤點單狀態",
            dataIndex: 'CHK_STATUS',
            width: 100,
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 100,
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100,
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 100,
        }, {
            text: "電腦量",
            dataIndex: 'STORE_QTY',
            width: 100,
        }, {
            text: "盤點量",
            dataIndex: 'CHK_QTY',
            width: 100,
        }, {
            text: "差異量",
            dataIndex: 'GAP_T',
            width: 100,
        }, {
            header: "",
            flex: 1
        }]
    });

    //#endregion

    //#region T3Grid
    var T3_chkStatusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var whno_Combo_3 = Ext.create('WEBAPP.form.WH_NoCombo', {
        //name: 'WH_NO',
        name: 'P5',
        id: 'P3P5',
        fieldLabel: '庫房代碼',
        width: 230,
        labelWidth: 70,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0041/GetWhnoCombo',

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                if (r.get('WH_NO') !== '') {
                    T3Query.getForm().findField('P5').setValue(r.get('WH_NO'));
                }

            }
        }
    });
    var mmCodeCombo_3 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P6',
        id: 'P3P6',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 230,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0041/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                if (r.get('MMCODE') !== '') {
                    T3Query.getForm().findField('P6').setValue(r.get('MMCODE'));
                }
            }
        }
    });
    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            items: [

                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P3P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    value: new Date(),
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    store: unitClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '單位類別',
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P3P1',
                    allowBlank: false,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 200,
                    labelWidth: 62,
                    fieldCls: 'required',
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: chkTypeStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點類別',
                    queryMode: 'local',
                    name: 'P3',
                    id: 'P3P3',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: ' ',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: T3_chkStatusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '盤點狀態',
                    queryMode: 'local',
                    name: 'P4',
                    id: 'P3P4',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 162,
                    labelWidth: 62,
                    value: ' ',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T3Query.getForm().isValid()) {
                            T3Load(true);

                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        }
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP31',
            border: false,
            layout: 'hbox',
            items: [
                whno_Combo_3,
                mmCodeCombo_3,
                {
                    xtype: 'textfield',
                    fieldLabel: '初盤單號',
                    name: 'P7',
                    id: 'P3P7',
                    allowBlank: true,
                    maxLength: 30,          // 可輸入最大長度為100
                },
            ]
        },
        ]
    });
    var T3Store = viewModel.getStore('UndoneDatas');
    function T3Load(clearMsg) {
        T3Store.getProxy().setExtraParam("p0", T3Query.getForm().findField('P0').rawValue);
        T3Store.getProxy().setExtraParam("p1", T3Query.getForm().findField('P1').getValue());
        T3Store.getProxy().setExtraParam("p3", T3Query.getForm().findField('P3').getValue());
        T3Store.getProxy().setExtraParam("p4", T3Query.getForm().findField('P4').getValue());
        T3Store.getProxy().setExtraParam("p5", T3Query.getForm().findField('P5').getValue());
        T3Store.getProxy().setExtraParam("p6", T3Query.getForm().findField('P6').getValue());
        T3Store.getProxy().setExtraParam("p7", T3Query.getForm().findField('P7').getValue());

        T3Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    T3Store.on('load', function (store, options) {

        if (store.totalCount > 0) {
            Ext.getCmp('export_T3').enable();
        } else {
            Ext.getCmp('export_T3').disable();
        }
    });

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'export_T3',
                text: '匯出',
                handler: function () {

                    if (T3Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>單位類別</span>為必填');
                        return;
                    }
                    var fn = T3Query.getForm().findField('P0').rawValue + '_未完成盤點明細.xls';

                    var p = new Array();
                    p.push({ name: 'FN', value: fn });
                    p.push({ name: 'p0', value: T3Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T3Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p3', value: T3Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T3Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T3Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T3Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T3Query.getForm().findField('P7').getValue() });

                    PostForm(T3GetExcel, p);
                },
                disabled: true
            }
        ]
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'panel',
                items: [T3Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 150,

        }, {
            text: "盤點類別",
            dataIndex: 'CHK_TYPE',
            width: 120,
        }, {
            text: "盤點期",
            dataIndex: 'CHK_PERIOD',
            width: 80,
        }, {
            text: "初盤單號",
            dataIndex: 'CHK_NO',
            width: 100,
        }, {
            text: "盤點年月",
            dataIndex: 'CHK_YM',
            width: 100,
        }, {
            text: "最後盤點階段",
            dataIndex: 'CHK_LEVEL',
            width: 100,
        }, {
            text: "盤點單狀態",
            dataIndex: 'CHK_STATUS',
            width: 100,
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 100,
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100,
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 100,
        }, {
            text: "電腦量",
            dataIndex: 'STORE_QTY',
            width: 100,
        }, {
            text: "盤點量",
            dataIndex: 'CHK_QTY',
            width: 100,
        }, {
            text: "差異量",
            dataIndex: 'GAP_T',
            width: 100,
        }, {
            header: "",
            flex: 1
        }]
    });
    //#endregion

    //#region tab
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                
                currentTab = newCard.itemId;
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            itemId: 't1Grid',
            title: '各庫盤點情況',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't2Grid',
            title: '已完成盤點明細資料',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T2Grid]
            },
        //    {
        //        itemId: 't3Grid',
        //        title: '未完成盤點明細資料',
        //        layout: 'border',
        //        padding: 0,
        //        split: true,
        //        region: 'center',
        //        layout: 'fit',
        //        collapsible: false,
        //        border: false,
        //        items: [T3Grid]
        //}
        ]
    });
    //#endregion

    //#region viewport
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [ {
            itemId: 'tabs',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [TATabs]
        }
        ]
    });
    //#endregion

    getUnitclassCombo();
    getChkTypeCombo();
    getChkStatusCombo();
    changeP2('Y');
 
});
