Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    //var reportUrl = '/Report/F/FA0055.aspx';
    var T1GetExcel = '/api/FA0055/Excel';
    // var T1Get = '/api/FA0055/All'; // 查詢(改為於store定義)
    var YMComboGet = '../../../api/FA0055/GetYMCombo';
    var TpeoOthUrl = '../../../api/FA0055/GetTpeoOth';
    var T1Name = "民診處藥局藥材收支結存及營收狀況表";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var YMQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setYMComboData() {
        Ext.Ajax.request({
            url: YMComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var set_ym = data.etts;
                    if (set_ym.length > 0) {
                        for (var i = 0; i < set_ym.length; i++) {
                            YMQueryStore.add({ VALUE: set_ym[i].VALUE, TEXT: set_ym[i].TEXT });
                            if (i == 0) {
                                tmpYM = set_ym[i];
                            }
                        }
                    }
                    T1Query.getForm().findField('P0').setValue(tmpYM);  //預設第一筆最近年月
                    
                    setTpeoOth(tmpYM);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYMComboData();

    function setTpeoOth() {
        Ext.Ajax.request({
            url: TpeoOthUrl,
            method: reqVal_p,
            params: { P0: T1Query.getForm().findField('P0').getValue() },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var items = data.etts;
                    if (items.length > 0) {
                        
                        T1Query.getForm().findField('P1').setValue(items[0].F1);
                        T1Query.getForm().findField('P2').setValue(items[0].F2);
                        T1Query.getForm().findField('P3').setValue(items[0].F3);
                        T1Query.getForm().findField('P4').setValue(items[0].F4);
                    } else {
                        T1Query.getForm().findField('P1').setValue('');
                        T1Query.getForm().findField('P2').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var mLabelWidth = 70;
    var mWidth = 150;

    // 查詢欄位
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
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '資料年月',
                            name: 'P0',
                            id: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            store: YMQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            editable: false,
                            listeners: {
                                select: function (oldValue, newValue, eOpts) {
                                    
                                    if (newValue != null) {
                                        setTpeoOth(newValue.data.VALUE);
                                    }
                                }
                            }
                        },
                        
                        {
                            xtype: 'button',
                            text: '查詢',
                            margin: '0 0 0 20',
                            handler: function () {
                                if (!T1Query.getForm().findField('P0').getValue() ) {
                                    Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                                    return;
                                }

                                T1Load();
                                msglabel('');
                            }
                        },
                        {
                            xtype: 'button',
                            text: '匯出',
                            id: 'T1xls',
                            handler: function () {
                                if (!T1Query.getForm().findField('P0').getValue() ||
                                    !T1Query.getForm().findField('P1').getValue() ||
                                    !T1Query.getForm().findField('P2').getValue()) {
                                    Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                                    return;
                                }

                                var p = new Array();
                                p.push({ name: 'FN', value: (T1Query.getForm().findField("P0").getValue() + '_民診處藥局藥材收支結存及營收狀況表.xls') });
                                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                                PostForm(T1GetExcel, p);
                                msglabel('匯出完成');
                            }
                        },
                        {
                            xtype: 'button',
                            text: '列印營收狀況表',
                            id: 'Print1',
                            handler: function () {
                                if (!T1Query.getForm().findField('P0').getValue()) {
                                    Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                                    return;
                                }

                                showReport('/Report/F/FA0055.aspx', T1Query.getForm().findField('P0').getValue());

                            }
                        },
                        {
                            xtype: 'button',
                            text: '列印備註報表',
                            id: 'Print2',
                            handler: function () {
                                if (!T1Query.getForm().findField('P0').getValue()) {
                                    Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                                    return;
                                }

                                showReport('/Report/F/FA0055_1.aspx', T1Query.getForm().findField('P0').getValue());
                            }
                        },
                        {
                            xtype: 'button',
                            text: '修改金額',
                            id: 'Update',
                            handler: function () {
                                if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                                    Ext.Msg.alert('提醒', '年月不可空白');
                                    msglabel("年月不可空白");
                                    return;
                                }
                                var f = T1Form.getForm();
                                f.findField('P0').setValue(T1Query.getForm().findField("P0").getValue());
                                f.findField('P1').setValue(T1Query.getForm().findField("P1").getValue());
                                f.findField('P2').setValue(T1Query.getForm().findField("P2").getValue());
                                f.findField('P3').setValue(T1Query.getForm().findField("P3").getValue());
                                f.findField('P4').setValue(T1Query.getForm().findField("P4").getValue());

                                editWindow.show();
                            }
                        }, {
                            xtype: 'button',
                            text: '2-4級庫調整金額明細匯出',
                            handler: function () {
                                if (T1Query.getForm().findField('P0').getValue() == '' ||
                                    T1Query.getForm().findField('P0').getValue() == null) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>月結年月</span>為必填');
                                    return;
                                }

                                var fn = T1Query.getForm().findField('P0').getValue() + '_2-4級庫調整金額明細.xls'
                                var p = new Array();
                                p.push({ name: 'FN', value: fn }); //檔名
                                p.push({ name: 'data_ym', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                                PostForm('/api/FA0055/GetAdjCost24Excel', p);
                            }
                        }, {
                            xtype: 'button',
                            text: '其他耗量明細匯出',
                            handler: function () {
                                if (T1Query.getForm().findField('P0').getValue() == '' ||
                                    T1Query.getForm().findField('P0').getValue() == null) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>月結年月</span>為必填');
                                    return;
                                }

                                var fn = T1Query.getForm().findField('P0').getValue() + '_其他耗量明細.xls'
                                var p = new Array();
                                p.push({ name: 'FN', value: fn }); //檔名
                                p.push({ name: 'data_ym', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                                PostForm('/api/FA0055/GetOtherDetailsExcel', p);
                            }
                        },
                        , {
                            xtype: 'displayfield',
                            fieldLabel: 'HIS2急診 11111 上線',
                            labelWidth: 150,
                            labelSeparator: '',
                            labelStyle: 'color: gray;',
                        },
                        //{
                        //    xtype: 'button',
                        //    text: '其他消耗成本明細表匯出',
                        //    handler: function () {
                        //        if (!T1Query.getForm().findField('P0').getValue()) {
                        //            Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                        //            return;
                        //        }

                        //        var p = new Array();
                        //        p.push({ name: 'FN', value: (T1Query.getForm().findField("P0").getValue() + '_其他消耗成本明細表.xlsx') });
                        //        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                        //        PostForm('', p);
                        //        msglabel('匯出完成');
                        //    }
                        //},
                        {
                            xtype: 'component',
                            flex: 1
                        },
                        {
                            xtype: 'button',
                            text: '明細',
                            handler: function () {
                                popWinForm();
                            }
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '台北門診中心買藥金額',
                            name: 'P1',
                            labelWidth: 150,
                            width: 240
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '本院買(管制藥)金額',
                            name: 'P2',
                            labelWidth: 150,
                            width: 240
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '本院買(抗瘧藥)金額',
                            name: 'P3',
                            labelWidth: 150,
                            width: 240
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '本院買(蛇毒血清)金額',
                            name: 'P4',
                            labelWidth: 150,
                            width: 240
                        },
                    ]
                }
            ]
        },
        ]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },

            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F9', type: 'string' },
            { name: 'F10', type: 'string' },

            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' },
            { name: 'F13', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0055/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0,
                first: 0
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        id:'T1Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }
            //, {
            //dock: 'top',
            //xtype: 'toolbar',
            //items: [T1Tool]
            //}
        ],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "月份",
            dataIndex: 'F1',
            width: 50
        }, {
            text: "上月結存成本",
            dataIndex: 'F2',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }

        }, {
            text: "本月買進成本",
            dataIndex: 'F3',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "本月醫令消耗成本",
            dataIndex: 'F4',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "藥局盤盈虧調整金額",
            dataIndex: 'F5',
            style: 'text-align:left',
            width: 140, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "銷毀過效期品",
            dataIndex: 'F6',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "退貨金額",
            dataIndex: 'F7',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "2-4級庫調整金額",
            dataIndex: 'F8',
            style: 'text-align:left',
            width: 140, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "本月結存成本",
            dataIndex: 'F9',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "調整金額",
            dataIndex: 'F10',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "藥材成本",
            dataIndex: 'F11',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "住院藥費消耗成本",
            dataIndex: 'F12',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "門診藥費消耗成本",
            dataIndex: 'F13',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "急診藥費消耗成本",
            dataIndex: 'F24',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "其他消耗成本(L)",
            dataIndex: 'F17',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "藥材總收入",
            dataIndex: 'F14',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "台北門診中心買藥金額",
            dataIndex: 'F15',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "本院買(管制藥、抗瘧藥、蛇毒血清)金額",
            dataIndex: 'F16',
            style: 'text-align:left',
            width: 120, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        },{
            header: "",
            flex: 1
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            }
        }
    });

    function showReport(url, data_ym) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + url + '?DATA_YM=' + data_ym
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

    //view 
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
                        items: [T1Grid]
                    }
                ]
            }]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();

    //#region editWindow

    function callSP(data_ym, tpeo, oth, oth1, oth2) {
        var myMask = new Ext.LoadMask(Ext.getCmp('T1Grid'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/FA0055/Apply',
            method: reqVal_p,
            params: {
                P0: data_ym,
                P1: tpeo,
                P2: oth,
                P3: oth1,
                P4: oth2
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    setTpeoOth(T1Query.getForm().findField('P0').getValue());
                    msglabel('資料更新成功，請重新查詢');
                    editWindow.hide();
                }
                else {
                    console.log("apply fail");
                }
            },
            failure: function (response, options) {

            }
        });
    }


    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 130,
            width: 300,
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'P0',
                            fieldLabel: '資料年月',
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '台北門診中心買藥金額',
                            name: 'P1',
                            id: 'P1',
                            allowBlank: false,
                            fieldCls: 'required',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 20,          // 可輸入最大長度為100
                            width: 300,
                            //labelWidth: 150,
                            hideTrigger: true,
                            decimals: 5
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '本院買(管制藥)金額',
                            name: 'P2',
                            id: 'P2',
                            allowBlank: false,
                            fieldCls: 'required',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 20,          // 可輸入最大長度為100
                            //width: 380,
                            //labelWidth: 230,
                            hideTrigger: true,
                            decimals: 5
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '本院買(抗瘧藥)金額',
                            name: 'P3',
                            id: 'P3',
                            allowBlank: false,
                            fieldCls: 'required',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 20,          // 可輸入最大長度為100
                            //width: 380,
                            //labelWidth: 230,
                            hideTrigger: true,
                            decimals: 5
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '本院買(蛇毒血清)金額',
                            name: 'P4',
                            id: 'P4',
                            allowBlank: false,
                            fieldCls: 'required',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 20,          // 可輸入最大長度為100
                            //width: 380,
                            //labelWidth: 230,
                            hideTrigger: true,
                            decimals: 5
                        },
                    ]
                },
            ]
        }]
    });
    var editWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T1Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "金額設定",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    
                    if (T1Form.getForm().findField('P1').getValue() == null ||
                        T1Form.getForm().findField('P2').getValue() == null ||
                        T1Form.getForm().findField('P3').getValue() == null ||
                        T1Form.getForm().findField('P4').getValue() == null ) {
                        Ext.Msg.alert('提醒', '金額欄位為必填');
                        return;
                    }
                    callSP(T1Query.getForm().findField('P0').getValue(),
                        T1Form.getForm().findField('P1').getValue(),
                        T1Form.getForm().findField('P2').getValue(),
                        T1Form.getForm().findField('P3').getValue(),
                        T1Form.getForm().findField('P4').getValue(),);
                   
                }
            },
            {
                text: '取消',
                handler: function () {
                    editWindow.hide();
                }
            }
        ]
    });
    editWindow.hide();
    //#endregion

    //#region 明細
    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    var callableWin = null;
    popWinForm = function () {
        var title = '';
        title = '藥品消耗月結報表 明細資料';

        var strUrl = "FA0055_Detail?data_ym=" + T1Query.getForm().findField('P0').getValue();
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion

    //#region 明細
    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    var callableWin = null;
    popWinForm_F17 = function () {
        var title = '';
        title = '其他消耗成本明細表';

        var strUrl = "FA0055_F17?data_ym=" + T1Query.getForm().findField('P0').getValue();
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion
});
