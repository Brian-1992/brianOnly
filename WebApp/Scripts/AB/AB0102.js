Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '/api/AB0102/Excel';
    var T3GetExcel = '/api/AB0102/DetailExcel';
    var mLabelWidth = 60;
    var mWidth = 150;
    var radioSelected = 'isCombo';
    var isRadioChanged = false;
    var currentTab = 't1Grid';
    
    var viewModel = Ext.create('WEBAPP.store.AB.AB0102VM');

    var whnoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P2_Combo',
        width: 100,
        allowBlank: true,
        labelWidth: 51,
        padding: ' 0 0 0 4',
        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            
            return {
                //WH_NO: tmpArray[0]
                //p1: T1Query.getForm().findField('GRP_NO').getValue(),
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0102/Whnos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T1Query.getForm();
                if (r.get('WH_NO') !== '') {
                    f.findField("P2_Combo").setValue(r.get('WH_NO'));
                }
            },
            change: function (field, newValue, oldValue) {
            }
        }
    });
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 180,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0102/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("P3").setValue(r.get('MMCODE'));
                }
            }
        }
    });
    var procMsgStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function getProcMsgCombo() {
        Ext.Ajax.request({
            url: '/api/AB0102/GetProcMsgCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var msgs = data.etts;
                    if (msgs.length > 0) {
                        procMsgStore.add({
                            VALUE: '',
                            TEXT: ''
                        });
                        for (var i = 0; i < msgs.length; i++) {
                            procMsgStore.add({
                                VALUE: msgs[i].VALUE,
                                TEXT: msgs[i].TEXT
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getProcMsgCombo();

    //#region T1Query
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
                    xtype: 'datefield',
                    fieldLabel: '資料日期',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: mLabelWidth,
                    allowBlank: true,
                    value: new Date((new Date()).getFullYear(), (new Date()).getMonth(), 1),
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 98,
                    allowBlank: true,
                    padding: '0 4 0 4',
                    value: new Date(),
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房代碼',
                    labelWidth: 100,
                    width: 100
                },
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P2',
                    inputValue: 'dropdown',
                    id: 'radio1',
                    padding: '0 0 0 4',
                    width: 15,
                    checked: true,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isCombo';
                                T1Query.getForm().findField('P2_Combo').enable();
                                T1Query.getForm().findField('P2_Text').disable();
                            }
                        }
                    }
                },
                whnoCombo,
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P2',
                    inputValue: 'textfield',
                    id: 'radio2',
                    padding: '0 0 0 4',
                    width: 15,
                    checked: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isTextfield';
                                T1Query.getForm().findField('P2_Combo').disable();
                                T1Query.getForm().findField('P2_Text').enable();
                            }
                        }
                    }
                },
                {
                    xtype: 'textfield',
                    name: 'P2_Text',
                    labelField: '',
                    width: '90',
                    emptyText: '自行輸入',
                    disabled: true
                },

            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                mmCodeCombo,
                {
                    xtype: 'fieldcontainer',
                    fieldLabel: '扣庫異常',
                    defaultType: 'checkboxfield',
                    labelWidth: 110,
                    labelSeparator: '',
                    width: 150,
                    items: [
                        {
                            name: 'P4',
                            inputValue: '1',
                            checked: true
                        }
                    ]
                },
                {
                    xtype: 'combo',
                    store: procMsgStore,
                    name: 'P5',
                    id: 'P5',
                    labelWidth: 80,
                    width: 200,
                    fieldLabel: '扣庫異常原因',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        if (currentTab == 't1Grid') {
                            T1Load();
                        }
                        if (currentTab == 't3Grid') {
                            T3Load();
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        vChkYM = "";
                        f.reset();

                        f.findField('CHK_YM').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '最新統計',
                    handler: function () {
                        T2Load();
                        latestDataWindow.show();
                    }
                }, 
            ]
            }, {
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'docbutton',
                    text: '扣庫規則說明',
                    documentKey: 'AB0102',
                    margin:'0 0 0 20'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '請先確認扣庫規則後有疑慮再撥打電話，謝謝',
                    labelWidth: 300,
                    labelSeparator: '',
                    hidden: true,
                    labelStyle: 'color: red;font-size: 14px; font-weight:bold',
                },]
            }
        ]
    });
    //#endregion

    //#region 主檔
    //var T1Store = Ext.create('WEBAPP.store.AB.AB0102', {
    //    pageSize: 50, // 每頁顯示筆數
    //    listeners: {
    //        beforeload: function (store, options) {
    //            var np = {
    //                p0: T1Query.getForm().findField('P0').rawValue,
    //                p1: T1Query.getForm().findField('P1').rawValue,
    //                p2: radioSelected == 'isCombo' ?
    //                    T1Query.getForm().findField('P2_Combo').getValue() :
    //                    T1Query.getForm().findField('P2_Text').getValue(),
    //                p3: T1Query.getForm().findField('P3').getValue(),
    //                p4: T1Query.getForm().findField('P4').checked ? 'Y' : 'N',
    //                p5: T1Query.getForm().findField('P5').rawValue
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});

    var T1Store = viewModel.getStore('Master');

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '醫令扣庫查詢_主檔' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({
                        name: 'p2', value: radioSelected == 'isCombo' ?
                            T1Query.getForm().findField('P2_Combo').getValue() :
                            T1Query.getForm().findField('P2_Text').getValue()
                    });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').checked ? 'Y' : 'N' });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').rawValue });
                    
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //dock: 'top',
            //xtype: 'toolbar',
            //layout: 'fit',
            //items: [T1Query]
            //},
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "資料日期",
            dataIndex: 'DATA_DATE',
            width: 80
        }, {
            text: "資料開始時間",
            dataIndex: 'DATA_BTIME',
            width: 100
        }, {
            text: "資料結束時間",
            dataIndex: 'DATA_ETIME',
            width: 100
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100
        }, {
            text: "門急住診別",
            dataIndex: 'VISIT_KIND',
            width: 80
        }, {
            text: "消耗量",
            dataIndex: 'CONSUME_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "扣庫單位",
            dataIndex: 'STOCK_UNIT',
            width: 80
        }, {
            text: '健保耗量',
            dataIndex: 'INSU_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "自費耗量",
            dataIndex: 'HOSP_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "母藥醫令代碼",
            dataIndex: 'PARENT_ORDERCODE',
            width: 100
        }, {
            text: "母藥消耗總量",
            dataIndex: 'PARENT_CONSUME_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 95
        }, {
            text: "資料寫入時間",
            dataIndex: 'CREATEDATETIME',
            width: 100
        }, {
            text: "扣庫處理結果",
            dataIndex: 'PROC_ID',
            width: 100
        }, {
            text: "處理訊息",
            dataIndex: 'PROC_MSG',
            width: 100
        }, {
            text: "處理類別",
            dataIndex: 'PROC_TYPE',
            width: 100
        }, {
            header: "",
            flex: 1
        }]
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", radioSelected == 'isCombo' ?
            T1Query.getForm().findField('P2_Combo').getValue() :
            T1Query.getForm().findField('P2_Text').getValue()
        );
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').rawValue);

        T1Tool.moveFirst();
        msglabel('訊息區:');
    }
    //#endregion

    //#region 2020-10-12 新增 明細
    var T3Store = viewModel.getStore('Details');

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '醫令扣庫查詢_明細' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({
                        name: 'p2', value: radioSelected == 'isCombo' ?
                            T1Query.getForm().findField('P2_Combo').getValue() :
                            T1Query.getForm().findField('P2_Text').getValue()
                    });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').checked ? 'Y' : 'N' });
                    PostForm(T3GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            },
        ]
    });

    // 查詢結果資料列表
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //dock: 'top',
            //xtype: 'toolbar',
            //layout: 'fit',
            //items: [T1Query]
            //},
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "ID",
            dataIndex: 'ID',
            width: 80
        }, {
            text: "年月日",
            dataIndex: 'DATA_DATE',
            width: 80
        }, {
            text: "統計時間(起)",
            dataIndex: 'DATA_BTIME',
            width: 100
        }, {
            text: "統計時間(迄)",
            dataIndex: 'DATA_ETIME',
            width: 100
        }, {
            text: "扣庫地點",
            dataIndex: 'STOCKCODE',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 100
        }, {
            text: "院內代碼",
            dataIndex: 'ORDERCODE',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100
        }, {
            text: "總量(開藥為正，退藥為負)",
            dataIndex: 'SUMQTY',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "是否需扣庫",
            dataIndex: 'STOCKFLAG',
            width: 80
        }, {
            text: "門急住病歷號",
            dataIndex: 'CHARNO',
            width: 80
        }, {
            text: "掛號流水號",
            dataIndex: 'RID',
            width: 80
        }, {
            text: "開立日期",
            dataIndex: 'WORKDATE',
            width: 80
        }, {
            text: "開立時間",
            dataIndex: 'WORKTIME',
            width: 80
        }, {
            text: "修改日期",
            dataIndex: 'MODIFYDATE',
            width: 80
        }, {
            text: "修改時間",
            dataIndex: 'MODIFYTIME',
            width: 80
        }, {
            text: "刪除日期時間",
            dataIndex: 'CANCELDATETIME',
            width: 80
        }, {
            text: "科別代碼",
            dataIndex: 'SECTIONNO',
            width: 80
        }, {
            text: "掛號醫師",
            dataIndex: 'VSDR',
            width: 80
        }, {
            text: "是否刪除",
            dataIndex: 'CANCELFLAG',
            width: 80
        }, {
            text: "刪除人員",
            dataIndex: 'CANCELOPID',
            width: 80
        }, {
            text: "開立人員",
            dataIndex: 'PROCOPID',
            width: 80
        }, {
            text: "開立日期時間",
            dataIndex: 'PROCDATETIME',
            width: 80
        }, {
            text: "建立人員",
            dataIndex: 'CREATEOPID',
            width: 80
        }, {
            text: "建立日期時間",
            dataIndex: 'CREATEDATETIME',
            width: 80
        }, {
            text: "領藥號",
            dataIndex: 'DRUGNO',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
                text: "病人電腦編號",
            dataIndex: 'MEDNO',
            width: 80
        }, {
            text: "批號",
            dataIndex: 'BATCHNUM',
            width: 80
        }, {
            text: "效期",
            dataIndex: 'EXPIREDDATE',
            width: 80
        }, {
            text: "母藥醫令代碼",
            dataIndex: 'PARENT_ORDERCODE',
            width: 80
        }, {
            text: "母藥消耗總量",
            dataIndex: 'PARENT_CONSUME_QTY',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }
            , {
            text: "門急住診別",
            dataIndex: 'VISIT_KIND',
            width: 80
        }, {
            text: "領藥日期",
            dataIndex: 'SENDDATE',
            width: 80
        }
            , {
            text: "開立醫師",
            dataIndex: 'ORDERDR',
            width: 80
        }, {
            text: "IC卡卡號",
            dataIndex: 'INSULOOKSEQ',
            width: 80
        }, {
            text: "開立劑量",
            dataIndex: 'DOSE',
            width: 80
        }, {
            text: "頻率",
            dataIndex: 'FREQNO',
            width: 80
        }, {
            text: "途徑",
            dataIndex: 'PATHNO',
            width: 80
        }, {
            text: "天數",
            dataIndex: 'DAYS',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "是否急作",
            dataIndex: 'EMGFLAG',
            width: 80
        }, {
            text: "部位",
            dataIndex: 'REGION',
            width: 80
        }, {
            text: "是否自費",
            dataIndex: 'PAYFLAG',
            width: 80
        }, {
            text: "是否計價",
            dataIndex: 'COMPUTECODE',
            width: 80
        }, {
            text: "預設為零(ADDRATIO1)",
            dataIndex: 'ADDRATIO1',
            width: 80
        }, {
            text: "兒童加成(ADDRATIO2)",
            dataIndex: 'ADDRATIO2',
            width: 80
        }, {
            text: "健保急作加成(ADDRATIO3)",
            dataIndex: 'ADDRATIO3',
            width: 80
        }, {
            text: "健保費用類別(健保歸屬)",
            dataIndex: 'INSUCHARGEID',
            width: 80
        }, {
            text: "院內費用類別(院內歸屬)",
            dataIndex: 'HOSPCHARGEID',
            width: 80
        }, {
            text: "健保價",
            dataIndex: 'INSUAMOUNT',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "自費價",
            dataIndex: 'PAYAMOUNT',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "慢箋流水號",
            dataIndex: 'SLOWCARD',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "序號",
            dataIndex: 'BAGSEQNO',
            width: 80,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "開立總量",
            dataIndex: 'TOTALQTY',
            width: 80
        }, {
            text: "單位(ORDERUNIT)",
            dataIndex: 'ORDERUNIT',
            width: 80
        }, {
            text: "單位(ATTACHUNIT)",
            dataIndex: 'ATTACHUNIT',
            width: 80
        }
            , {
            header: "",
            flex: 1
        }]
    });

    function T3Load() {
        T3Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T3Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T3Store.getProxy().setExtraParam("p2", radioSelected == 'isCombo' ?
            T1Query.getForm().findField('P2_Combo').getValue() :
            T1Query.getForm().findField('P2_Text').getValue()
        );
        T3Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T3Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').checked ? 'Y' : 'N');
        T3Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').rawValue);

        T3Tool.moveFirst();
        msglabel('訊息區:');
    }
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
            title: '主檔',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't3Grid',
            title: '明細',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T3Grid]
        }]
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
        items: [{
            itemId: 'form',
            region: 'north',
            collapsible: false,
            floatable: false,
            collapsed: false,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Query]
        }, {
            itemId: 't1Grid',
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

    //#region 2020-09-14 最新統計
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            '門急住診別',
            '最新資料日期',
            '最新資料結束時間',
            '扣庫成功筆數',
            '扣庫失敗筆數',
            '系統時間'
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
    });
    function T2Load() {
        Ext.Ajax.request({
            url: '/api/AB0102/GetLatestData',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    var latest_datas = data.etts;
                    if (latest_datas.length > 0) {
                        for (var i = 0; i < latest_datas.length; i++) {
                            T2Store.add(latest_datas[i]);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "門急住診別",
            dataIndex: '門急住診別',
            width: 80
        }, {
            text: "最新資料日期",
            dataIndex: '最新資料日期',
            width: 100
        }, {
            text: "最新資料結束時間",
            dataIndex: '最新資料結束時間',
            width: 130
        }, {
            text: "扣庫成功筆數",
            dataIndex: '扣庫成功筆數',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "扣庫失敗筆數",
            dataIndex: '扣庫失敗筆數',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "系統時間",
            dataIndex: '系統時間',
            width: 130
        }]
    });

    var latestDataWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            T2Grid
        ],
        width: "710px",
        resizable: true,
        draggable: true,
        closable: false,
        title: "最新統計",
        buttons: [{
            text: '關閉',
            handler: function () {
                //myMask.hide();
                this.up('window').hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                latestDataWindow.center();
            }
        }
    });
    latestDataWindow.hide();
    //#endregion
});