Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = '醫令扣庫查詢';
    var T1GetExcel = '../../../api/AB0132/Excel';
    var T1GetExcel2 = '../../../api/AB0132/Excel2';
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');
    var BTime = "";
    //門急住診別
    var visitKindQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "1", "TEXT": "1 門診消耗" },
            { "VALUE": "2", "TEXT": "2 住院消耗" },
            { "VALUE": "3", "TEXT": "3 急診消耗" },
            { "VALUE": "4", "TEXT": "4 出院帶藥" }
        ]
    });
    //藥品代碼
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        fieldLabel: '藥品代碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0132/GetOrdercodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                menuLink: menuLink
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Query.getForm().findField('MMCODE_C').setValue(r.data.MMNAME_C);
            }
        }
    });
    //科別代碼
    var SetSectionNoComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0132/GetSectionNoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    //扣庫地點
    var st_whnoCombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0132/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    
    var mLabelWidth = 80;
    var mWidth = 200;
    //上方查詢區塊
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '0 4 0 4'
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'container',
                layout: 'hbox',
                items: [{
                    xtype: 'monthfield',
                    fieldLabel: '月份',
                    name: 'P0Mon',
                    id: 'P0Mon',
                    width: 160,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    hidden: true
                }, {
                    xtype: 'monthfield',
                    id: 'P1Mon',
                    name: 'P1Mon',
                    fieldLabel: '至',
                    labelWidth: 30,
                    width: 130,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    hidden: true
                },{
                    xtype: 'datefield',
                    fieldLabel: '日期',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    value: menuLink === "AB0149" ? Ext.util.Format.date(new Date(), "Y-m-d") : Ext.util.Format.date(new Date().addDays(-1), "Y-m-d"), //若為AB149則帶入兩個當天預設值
                    regexText: '請選擇日期',
                    hidden: true,
                    maxValue: Ext.util.Format.date(new Date(), "Y-m-d"), //設定最大選擇日期為今天
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            if (menuLink == "AB0149") {
                                T1Query.getForm().findField('P1').setValue(newValue); //若為AB0149，設定P1與P0同步變更日期
                            }
                        }
                    }
                }, {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '至',
                    labelWidth: 10,
                    labelSeparator: '',
                    fieldCls: 'required',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P0' },
                    value: Ext.util.Format.date(new Date(), "Y-m-d"),
                    hidden: true,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            if (menuLink != "AB0149") {
                                Ext.getCmp('P0').setMaxValue(newValue); //手動調整P0的可選擇日期上限
                            }
                        }
                    }
                }, {
                    xtype: 'combo',
                    store: visitKindQueryStore,
                    name: 'P2',
                    id: 'P2',
                    fieldLabel: '門急住診別',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    value: ''
                }, T1QueryMMCode,
                {
                    xtype: 'displayfield',
                    fieldLabel: '藥品名稱',
                    name: 'MMCODE_C',
                    id: 'MMCODE_C',
                    width: 350
                }]
            }, {
                xtype: 'container',
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '病歷號碼',
                        name: 'P4',
                        id: 'P4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '科別代碼',
                        name: 'P5',
                        id: 'P5',
                        store: SetSectionNoComboGet,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '護理站代碼',
                        name: 'P6',
                        id: 'P6'
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '床位號',
                        name: 'P7',
                        id: 'P7'
                    },  {
                        xtype: 'combo',
                        store: st_whnoCombo,
                        name: 'P8',
                        id: 'P8',
                        fieldLabel: '扣庫地點',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        //value: ''
                    },{
                        xtype: 'button',
                        margin: '0 0 0 40',
                        text: '查詢',
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (f.isValid()) {
                                msglabel('訊息區:');
                                T1Load();
                            }
                            else {
                                Ext.Msg.alert('訊息', '查詢欄位填寫格式錯誤');
                            }
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            T1Query.getForm().findField('P2').setValue('1');

                            if (menuLink == "AB0151") {
                                f.findField('P0Mon').focus(); // 進入畫面時輸入游標預設在P0Mon
                                SetBTime(); //重置AB0151時間
                            }
                            else if (menuLink == "AB0152") {
                                f.findField('P0Mon').focus(); // 進入畫面時輸入游標預設在P0Mon
                                var d = new Date(); //重置AB0152時間
                                m = d.getMonth(); //current month
                                y = d.getFullYear(); //current year
                                T1Query.getForm().findField('P0Mon').setValue(new Date(y, m));
                                T1Query.getForm().findField('P1Mon').setValue(new Date(y, m));
                            }
                            else if (menuLink == "AB0149") { //重置AB0149時間
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                T1Query.getForm().findField('P0').setValue(Ext.util.Format.date(new Date(), "Y-m-d"));
                                T1Query.getForm().findField('P1').setValue(Ext.util.Format.date(new Date(), "Y-m-d"));
                            }
                            else {
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                T1Query.getForm().findField('P0').setValue(Ext.util.Format.date(new Date().addDays(-1), "Y-m-d"));
                                T1Query.getForm().findField('P1').setValue(Ext.util.Format.date(new Date(), "Y-m-d"));
                            }
                        }
                    }]
            }]
        }]
    });
    var T1Store = Ext.create('WEBAPP.store.AB0132', {
        listeners: {
            beforeload: function (store, options) {
                //設定不同的p0,p1值(AB0151,152只有月份)
                if (menuLink != "AB0151" && menuLink != "AB0152") {
                    tmpP0 = T1Query.getForm().findField('P0').rawValue;
                    tmpP1 = T1Query.getForm().findField('P1').rawValue;
                } else {
                    tmpP0 = T1Query.getForm().findField('P0Mon').rawValue;
                    tmpP1 = T1Query.getForm().findField('P1Mon').rawValue;
                }
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: tmpP0,
                    p1: tmpP1,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    menuLink: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        msglabel('查無資料!');
                    }
                }
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    //設定不同的p0,p1值(AB0151,152只有月份)
                    if (menuLink != "AB0151" && menuLink != "AB0152") {
                        tmpP0 = T1Query.getForm().findField('P0').rawValue;
                        tmpP1 = T1Query.getForm().findField('P1').rawValue;
                    } else {
                        tmpP0 = T1Query.getForm().findField('P0Mon').rawValue;
                        tmpP1 = T1Query.getForm().findField('P1Mon').rawValue;
                    }
                    p.push({ name: 'p0', value: tmpP0 });
                    p.push({ name: 'p1', value: tmpP1 });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() });
                    p.push({ name: 'p8', value: T1Query.getForm().findField('P8').getValue() });
                    p.push({ name: 'menuLink', value: menuLink });
                    p.push({ name: 'FN', value: '醫令扣庫查詢.xlsx' });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {                
                itemId: 't1export2', text: '匯出總和',
                handler: function () {       
                    //設定不同的p0,p1值(AB0151,152只有月份)
                    if (menuLink != "AB0151" && menuLink != "AB0152") {
                        tmpP0 = T1Query.getForm().findField('P0').rawValue;
                        tmpP1 = T1Query.getForm().findField('P1').rawValue;
                    } else {
                        tmpP0 = T1Query.getForm().findField('P0Mon').rawValue;
                        tmpP1 = T1Query.getForm().findField('P1Mon').rawValue;
                    }

                    Ext.Msg.show({
                        title: '匯出總和',
                        message: '日期起訖 : ' + tmpP0 + '~' + tmpP1 + '<br>'
                        + '門急住診別 : ' + T1Query.getForm().findField('P2').rawValue + '<br>'
                        + '藥品代碼 : ' + T1Query.getForm().findField('P3').rawValue + '<br>'
                        +'<br>'
                                + '是否匯出?:',
                        buttons: Ext.Msg.YESNO,
                        icon: Ext.Msg.QUESTION,
                        fn: function (btn) {
                            if (btn === 'yes') {
                                var p = new Array();
                                p.push({ name: 'p0', value: tmpP0 });
                                p.push({ name: 'p1', value: tmpP1 });
                                p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                                p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                                p.push({ name: 'menuLink', value: menuLink });
                                p.push({ name: 'FN', value: '醫令扣庫查詢(總和).xlsx' });
                                PostForm(T1GetExcel2, p);
                                msglabel('訊息區:匯出完成');
                            } 
                        }
                    });

                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "日期",
                dataIndex: 'DATA_DATE',
                width: 80
            },
            {
                text: "藥品代碼",
                dataIndex: 'ORDERCODE',
                width: 80
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 250
            },
            {
                text: "開立醫師",
                dataIndex: 'ORDERDR',
                width: 80
            },
            {
                text: "病歷號碼",
                dataIndex: 'MEDNO',
                width: 100
            },
            {
                text: "科別代碼",
                dataIndex: 'SECTIONNO',
                width: 80
            },
            {
                text: "科別名稱",
                dataIndex: 'SECTIONNAME',
                width: 80
            },
            {
                text: "開立劑量",
                dataIndex: 'DOSE',
                width: 80
            },
            {
                text: "總量",
                dataIndex: 'SUMQTY',
                width: 80
            },
            {
                text: "建立人員",
                dataIndex: 'CREATEOPID',
                width: 80
            },
            {
                text: "建立日期時間",
                dataIndex: 'CREATEDATETIME',
                width: 100
            },
            {
                text: "單位",
                dataIndex: 'ORDERUNIT',
                width: 80
            },
            {
                text: "劑型單位",
                dataIndex: 'ATTACHUNIT',
                width: 80
            },
            {
                text: "門急住診別",
                dataIndex: 'VISIT_KIND',
                width: 100
            },
            {
                text: "成本",
                dataIndex: 'DET_COST_14',
                width: 80
            },
            {
                text: "成本中心部門",
                dataIndex: 'DET_DEPTCENTER_14',
                width: 100
            },
            {
                text: "護理站代碼",
                dataIndex: 'DET_NRCODE_14',
                width: 100
            },
            {
                text: "床位號",
                dataIndex: 'DET_BEDNO_14',
                width: 80
            },
            {
                text: "扣庫地點",
                dataIndex: 'STOCKCODE',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ]
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid
                ]
            }
        ]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    function SetBTime() {
        Ext.Ajax.request({
            url: '/api/AB0132/GetSetBTime',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    BTime = data.msg;
                    if (BTime != "") {
                        T1Query.getForm().findField('P0Mon').setValue(BTime.substr(0,5)); //設定AB0151月份起始

                        var d = new Date(); //設定AB0151月份結束
                        m = d.getMonth(); //current month
                        y = d.getFullYear(); //current year
                        T1Query.getForm().findField('P1Mon').setValue(new Date(y, m));
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function MenuLinkSet() {
        if (menuLink == "AB0151") {
            Ext.getCmp('P0Mon').show()
            Ext.getCmp('P1Mon').show()
            Ext.getCmp('P0Mon').allowBlank = false //設定必填
            Ext.getCmp('P1Mon').allowBlank = false
            SetBTime();
        }
        else if (menuLink == "AB0152") {
            Ext.getCmp('P0Mon').show()
            Ext.getCmp('P0Mon').allowBlank = false //設定必填

            var d = new Date(); //設定AB0152當月份
            m = d.getMonth(); //current month
            y = d.getFullYear(); //current year
            T1Query.getForm().findField('P0Mon').setValue(new Date(y, m));
            T1Query.getForm().findField('P1Mon').setValue(new Date(y, m));
        }
        else if (menuLink == "AB0149"){
            Ext.getCmp('P0').show()
            Ext.getCmp('P0').allowBlank = false //設定必填
        }
        else {
            Ext.getCmp('P0').show()
            Ext.getCmp('P1').show()
            Ext.getCmp('P0').allowBlank = false //設定必填
            Ext.getCmp('P1').allowBlank = false
        }
    }
    MenuLinkSet();
});