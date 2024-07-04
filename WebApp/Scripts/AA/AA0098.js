Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Get = '/api/AA0098/AllM'; // 查詢(改為於store定義)
    var T1Set = ''; 
    var T1Name = "HIS品項批次同步更新";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var ActNumber = 0; // 目前在處理T1Store的第幾筆
    var LastNumber = 0; 

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P2',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: mLabelWidth,
        width: mWidth,
        padding: '0 4 0 4',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0098/GetMMCodeDocd', //指定查詢的Controller路徑
        //getDefaultParams: function () { //查詢時Controller固定會收到的參數
        //    return {
        //        mat_class: '02'
        //    };
        //},
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    // 查詢欄位
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
    }

    var mLabelWidth = 70;
    var mWidth = 150;
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
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '物料分類',
                        name: 'P9',
                        id: 'P9',
                        labelWidth: mLabelWidth,
                        width: 120
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '異動日期',
                        labelAlign: 'right',
                        name: 'P0',
                        id: 'P0',
                        vtype: 'dateRange',
                        dateRange: { end: 'P1' },
                        padding: '0 4 0 4',
                        labelWidth: 80,
                        width: 170
                        //,value: get6monthday()
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelAlign: 'right',
                        labelWidth: 10,
                        name: 'P1',
                        id: 'P1',
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'P0' },
                        padding: '0 4 0 4',
                        width: 90
                        //,value: getToday()
                    }, T1QueryMMCode,
                    {
                        xtype: 'checkbox',
                        name: 'CK1',
                        boxLabel: '僅顯示差異',
                        inputValue: 'Y',
                        labelWidth: mLabelWidth,
                        width: 120,
                        checked: true
                    }, {
                        name: 'P3',
                        xtype: 'hidden'
                    }, {
                        xtype: 'button',
                        //text: '第一個',
                        iconCls: 'MMSMSfirst',
                        itemId: 'btnFirstPage',
                        labelAlign: 'center',
                        disabled: true,
                        handler: function () {
                            ActNumber = 0;
                            T1Form.getForm().findField('PAGENUM').setValue((ActNumber + 1) + '/' + T1Store.data.items.length);
                            if (ActNumber + 1 == 1)
                                T1Form.down('#btnPageUp').setDisabled(true);
                            else
                                T1Form.down('#btnPageUp').setDisabled(false);
                            T1Form.down('#btnPageDown').setDisabled(false);
                            T1Form.loadRecord(T1Store.data.items[ActNumber]);
                        }
                    }, {
                        xtype: 'button',
                        //text: '上一個',
                        iconCls: 'MMSMSPre',
                        itemId: 'btnPageUp',
                        labelAlign: 'center',
                        disabled: true,
                        handler: function () {
                            ActNumber = ActNumber - 1;
                            T1Form.getForm().findField('PAGENUM').setValue((ActNumber + 1) + '/' + T1Store.data.items.length);
                            if (ActNumber + 1 == 1)
                                T1Form.down('#btnPageUp').setDisabled(true);
                            else
                                T1Form.down('#btnPageUp').setDisabled(false);
                            T1Form.down('#btnPageDown').setDisabled(false);
                            T1Form.loadRecord(T1Store.data.items[ActNumber]);
                        }
                    }, {
                        xtype: 'displayfield',
                        name: 'PAGENUM',
                        width: null,
                        readOnly: true,
                        padding: '0 2 0 2',
                        value: ''
                    }, {
                        xtype: 'button',
                        //text: '下一個',
                        iconCls: 'MMSMSNext',
                        itemId: 'btnPageDown',
                        labelAlign: 'center',
                        disabled: true,
                        handler: function () {
                            ActNumber = ActNumber + 1;
                            T1Form.getForm().findField('PAGENUM').setValue((ActNumber + 1) + '/' + T1Store.data.items.length);
                            if (ActNumber + 1 == T1Store.data.items.length)
                                T1Form.down('#btnPageDown').setDisabled(true);
                            else
                                T1Form.down('#btnPageDown').setDisabled(false);
                            T1Form.down('#btnPageUp').setDisabled(false);

                            T1Form.loadRecord(T1Store.data.items[ActNumber]);
                        }
                    }, {
                        xtype: 'button',
                        //: '最後一個',
                        iconCls: 'MMSMSLast',
                        itemId: 'btnLastPage',
                        labelAlign: 'center',
                        disabled: true,
                        handler: function () {
                            ActNumber = LastNumber;
                            T1Form.getForm().findField('PAGENUM').setValue((ActNumber + 1) + '/' + T1Store.data.items.length);
                            if (ActNumber + 1 == T1Store.data.items.length)
                                T1Form.down('#btnPageDown').setDisabled(true);
                            else
                                T1Form.down('#btnPageDown').setDisabled(false);
                            T1Form.down('#btnPageUp').setDisabled(false);

                            T1Form.loadRecord(T1Store.data.items[ActNumber]);
                        }
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (f.findField('CK1').checked) {
                                f.findField('P3').setValue('Y');
                            }
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            T1Form.getForm().findField('PAGENUM').setValue('');
                            T1Form.getForm().reset();
                            T1Query.getForm().findField('P9').setValue('02 衛材');
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'radiogroup',
                        fieldLabel: '',
                        name: 'Type',
                        width: 120,
                        items: [
                            { boxLabel: '目前', width: 50, name: 'Type', inputValue: '0', checked: true },
                            { boxLabel: '次月', width: 50, name: 'Type', inputValue: '1' }
                        ]
                    },
                    {
                        xtype: 'button',
                        text: '儲存',
                        id: 'btnUpdate',
                        disabled: true,
                        handler: function () {
                            if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                                Ext.MessageBox.confirm('儲存', '是否確定儲存?', function (btn, text) {
                                    if (btn === 'yes') {
                                        if (T1Query.getForm().findField('Type').getValue()['Type'] == '0') {
                                            T1Set = '/api/AA0098/CreateM';
                                        }
                                        else {
                                            T1Set = '/api/AA0098/CreateN';
                                        }
                                        T1Submit();
                                    }
                                }
                                );
                            }
                        }
                    }
                ]
            }
        ]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    //T1Query.getForm().reset();
                    var v = action.result.etts[0];
                    T1Query.getForm().findField('P2').setValue(v.MMCODEB);
                    r.set(v);
                    T1Store.insert(0, r);
                    r.commit();
                    msglabel('訊息區:資料儲存成功');
                    T1Load();
                    f2.findField('PAGENUM').setValue('');
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    //T1FormBtnEnable('2');
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
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            //MI_MAST
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'AUTO_APLID', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_IDKEY', type: 'string' },
            { name: 'M_INVKEY', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'M_GOVKEY', type: 'string' },
            { name: 'M_VOLL', type: 'string' },
            { name: 'M_VOLW', type: 'string' },
            { name: 'M_VOLH', type: 'string' },
            { name: 'M_VOLC', type: 'string' },
            { name: 'M_SWAP', type: 'string' },
            { name: 'M_MATID', type: 'string' },
            { name: 'M_SUPPLYID', type: 'string' },
            { name: 'M_CONSUMID', type: 'string' },
            { name: 'M_PAYKIND', type: 'string' },
            { name: 'M_PAYID', type: 'string' },
            { name: 'M_TRNID', type: 'string' },
            { name: 'M_APPLYID', type: 'string' },
            { name: 'M_PHCTNCO', type: 'string' },
            { name: 'M_ENVDT', type: 'string' },
            { name: 'M_DISTUN', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'M_AGENLAB', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'M_DISCPERC', type: 'string' },
            { name: 'E_SUPSTATUS', type: 'string' },
            { name: 'E_MANUFACT', type: 'string' },
            { name: 'E_IFPUBLIC', type: 'string' },
            { name: 'E_STOCKTYPE', type: 'string' },
            { name: 'E_SPECNUNIT', type: 'string' },
            { name: 'E_COMPUNIT', type: 'string' },
            { name: 'E_YRARMYNO', type: 'string' },
            { name: 'E_ITEMARMYNO', type: 'string' },
            { name: 'E_GPARMYNO', type: 'string' },
            { name: 'E_CLFARMYNO', type: 'string' },
            { name: 'E_CODATE', type: 'string' },
            { name: 'E_PRESCRIPTYPE', type: 'string' },
            { name: 'E_DRUGCLASS', type: 'string' },
            { name: 'E_DRUGCLASSIFY', type: 'string' },
            { name: 'E_DRUGFORM', type: 'string' },
            { name: 'E_COMITMEMO', type: 'string' },
            { name: 'E_COMITCODE', type: 'string' },
            { name: 'E_INVFLAG', type: 'string' },
            { name: 'E_PURTYPE', type: 'string' },
            { name: 'E_SOURCECODE', type: 'string' },
            { name: 'E_DRUGAPLTYPE', type: 'string' },
            { name: 'E_ARMYORDCODE', type: 'string' },
            { name: 'E_PARCODE', type: 'string' },
            { name: 'E_PARORDCODE', type: 'string' },
            { name: 'E_SONTRANSQTY', type: 'string' },
            { name: 'CANCEL_ID', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }, 
            { name: 'E_PATHNO', type: 'string' },
            { name: 'E_ORDERUNIT', type: 'string' },
            { name: 'E_FREQNOO', type: 'string' },
            { name: 'E_FREQNOI', type: 'string' },
            { name: 'CONTRACNO', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            //V_HIS_MAST A,MI_MAST B
            { name: 'MMCODEA', type: 'string' },
            { name: 'MMCODEB', type: 'string' },
            { name: 'MMNAME_EA', type: 'string' },
            { name: 'MMNAME_EB', type: 'string' },
            { name: 'MMNAME_CA', type: 'string' },
            { name: 'MMNAME_CB', type: 'string' },
            { name: 'BASE_UNITA', type: 'string' },
            { name: 'BASE_UNITB', type: 'string' },
            { name: 'M_PURUNA', type: 'string' },
            { name: 'M_PURUNB', type: 'string' },
            { name: 'M_APPLYIDA', type: 'string' },
            { name: 'M_APPLYIDB', type: 'string' },
            { name: 'M_PHCTNCOA', type: 'string' },
            { name: 'M_PHCTNCOB', type: 'string' },
            { name: 'M_AGENLABA', type: 'string' },
            { name: 'M_AGENLABB', type: 'string' },
            { name: 'UNIT_SWAP', type: 'string' },
            { name: 'EXCH_RATIO', type: 'string' },
            { name: 'M_DISCPERCA', type: 'string' },
            { name: 'M_DISCPERCB', type: 'string' },
            { name: 'DISC_CPRICEA', type: 'string' },
            { name: 'DISC_CPRICEB', type: 'string' },
            { name: 'UPRICEA', type: 'string' },
            { name: 'UPRICEB', type: 'string' },
            { name: 'M_CONTPRICEA', type: 'string' },
            { name: 'M_CONTPRICEB', type: 'string' },
            { name: 'M_NHIKEYA', type: 'string' },
            { name: 'M_NHIKEYB', type: 'string' },
            { name: 'M_AGENNOA', type: 'string' },
            { name: 'M_AGENNOB', type: 'string' },
            { name: 'M_MATIDA', type: 'string' },
            { name: 'M_MATIDB', type: 'string' },
            { name: 'M_CONTIDB', type: 'string' },
            { name: 'PROCDATETIME', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 99999,
        remoteSort: true,
        sorters: [{ property: 'MMCODEA', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                var myMask = new Ext.LoadMask(viewport, { msg: '讀取中...' });
                myMask.show();
                if (!successful) {
                    T1Store.removeAll();

                    myMask.hide();
                }
                else {
                    if (records.length > 0) {
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        T1Cleanup(); msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T1FormBtnEnable('2');
                    }
                    myMask.hide();
                }
                var findIdx = store.find('MMCODE', T1LastRec.data['MMCODE']);
                if (findIdx < 0)
                    findIdx = 0;
                T1Form.loadRecord(T1Store.data.items[findIdx]);

                // 若院內碼筆數大於1,顯示換頁鈕
                if (T1Store.data.items.length > 1) {

                    T1Form.down('#btnFirstPage').setDisabled(false);
                    T1Form.down('#btnPageUp').setDisabled(false);
                    T1Form.down('#btnPageDown').setDisabled(false);
                    T1Form.down('#btnLastPage').setDisabled(false);
                    LastNumber = T1Store.data.items.length - 1;
                    T1Form.down('#btnPageDown').setDisabled(false);
                    //T1Form.down('#pageButtons').setVisible(true);
                    if (ActNumber + 1 == 1)
                        T1Form.down('#btnPageUp').setDisabled(true);
                    else
                        T1Form.down('#btnPageUp').setDisabled(false);
                    if (ActNumber + 1 == T1Store.data.items.length)
                        T1Form.down('#btnPageDown').setDisabled(true);
                    else
                        T1Form.down('#btnPageDown').setDisabled(false);
                    T1Form.getForm().findField('PAGENUM').setValue('1/' + T1Store.data.items.length);

                }
                else {
                    if (T1Store.data.items.length == 1) {
                        T1Form.getForm().findField('PAGENUM').setValue('1/1');
                    }
                    else {

                    }
                    T1Form.down('#btnFirstPage').setDisabled(true);
                    T1Form.down('#btnPageUp').setDisabled(true);
                    T1Form.down('#btnPageDown').setDisabled(true);
                    T1Form.down('#btnLastPage').setDisabled(true);
                }
                T1FormBtnEnable('1');
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T1Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        //var myMask = new Ext.LoadMask(viewport, { msg: '讀取中...' });
        //myMask.show();
        T1Store.load(
            //{
            //    success: function () {
            //        
            //        myMask.hide();
            //    },
            //    failure: function () {
            //        
            //        myMask.hide();
            //    },
            //    callback: function () {
            //        
            //        myMask.hide();
            //    }
            //}
        );
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 4,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '30%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            labelWidth: 90
        },
        defaultType: 'textfield',
        dockedItems: [{
            dock: 'top', 
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }],
        items: [{
            name: 'x',
            xtype: 'hidden'
            }, {
                xtype: 'label',
                text: '項目',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: 'HIS資料',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '藥衛材資料',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '院內碼',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMCODEA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMCODEB',
                readOnly: true
            }, {
                xtype: 'button',
                text: '全部複製',
                padding: '0 20 5 20',
                id: 'F01btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    var v_str2 = "";
                    var v_str3 = "";
                    T1Form.getForm().findField('MMCODEB').setValue(T1Form.getForm().findField('MMCODEA').getValue());
                    v_str = T1Form.getForm().findField('MMNAME_EA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('MMNAME_EB').setValue(v_str);
                    v_str = T1Form.getForm().findField('MMNAME_CA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('MMNAME_CB').setValue(v_str);
                    T1Form.getForm().findField('BASE_UNITB').setValue(T1Form.getForm().findField('BASE_UNITA').getValue());
                    T1Form.getForm().findField('M_PURUNB').setValue(T1Form.getForm().findField('M_PURUNA').getValue());
                    v_str = T1Form.getForm().findField('M_APPLYIDA').getValue();
                    if (v_str == "Y") {
                        v_str2 = "E";
                    }
                    else {
                        v_str2 = "Y";
                    }
                    T1Form.getForm().findField('M_APPLYIDB').setValue(v_str2);

                    v_str = T1Form.getForm().findField('M_PHCTNCOA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('M_PHCTNCOB').setValue(v_str);
                    T1Form.getForm().findField('M_AGENLABB').setValue(T1Form.getForm().findField('M_AGENLABA').getValue());
                    T1Form.getForm().findField('EXCH_RATIO').setValue(T1Form.getForm().findField('UNIT_SWAP').getValue());
                    T1Form.getForm().findField('M_DISCPERCB').setValue(T1Form.getForm().findField('M_DISCPERCA').getValue());
                    T1Form.getForm().findField('DISC_CPRICEB').setValue(T1Form.getForm().findField('DISC_CPRICEA').getValue());
                    T1Form.getForm().findField('UPRICEB').setValue(T1Form.getForm().findField('UPRICEA').getValue());
                    T1Form.getForm().findField('M_CONTPRICEB').setValue(T1Form.getForm().findField('M_CONTPRICEA').getValue());
                    T1Form.getForm().findField('M_NHIKEYB').setValue(T1Form.getForm().findField('M_NHIKEYA').getValue());
                    T1Form.getForm().findField('M_AGENNOB').setValue(T1Form.getForm().findField('M_AGENNOA').getValue());

                    v_str = T1Form.getForm().findField('M_MATIDA').getValue();
                    if (v_str == "1" || v_str == "2") {
                        v_str2 = "0";
                        v_str3 = "Y";
                    }
                    else {
                        if (v_str == "0") {
                            v_str2 = "3";
                            v_str3 = "N";
                        }
                        else {
                            v_str2 = "2";
                            v_str3 = "N";
                        }
                    }
                    T1Form.getForm().findField('M_MATIDB').setValue(v_str2);
                    T1Form.getForm().findField('M_CONTIDB').setValue(v_str3);
                    getDISC_UPRICE();
                }
            }, {
                xtype: 'label',
                text: '英文品名',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMNAME_EA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMNAME_EB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F02btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('MMNAME_EA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('MMNAME_EB').setValue(v_str);
                }
            }, {
                xtype: 'label',
                text: '中文品名',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMNAME_CA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'MMNAME_CB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F03btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('MMNAME_CA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('MMNAME_CB').setValue(v_str);
                }
            }, {
                xtype: 'label',
                text: '計量單位',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'BASE_UNITA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'BASE_UNITB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F04btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('BASE_UNITB').setValue(T1Form.getForm().findField('BASE_UNITA').getValue());
                }
            }, {
                xtype: 'label',
                text: '包裝單位',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_PURUNA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_PURUNB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F05btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('M_AGENNOA').getValue();
                    v_str.replace(" ", "");
                    T1Form.getForm().findField('M_AGENNOB').setValue(T1Form.getForm().findField('M_AGENNOA').getValue());
                    T1Form.getForm().findField('M_PURUNB').setValue(T1Form.getForm().findField('M_PURUNA').getValue());
                    T1Form.getForm().findField('EXCH_RATIO').setValue(T1Form.getForm().findField('UNIT_SWAP').getValue());
                }
            }, {
                xtype: 'label',
                text: '特殊識別碼',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_APPLYIDA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_APPLYIDB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F06btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    var v_str2 = "";
                    v_str = T1Form.getForm().findField('M_APPLYIDA').getValue();
                    if (v_str == "Y") {
                        v_str2 = "E";
                    }
                    else {
                        v_str2 = "Y";
                    }
                    T1Form.getForm().findField('M_APPLYIDB').setValue(v_str2);
                }
            }, {
                xtype: 'label',
                text: '衛署字號',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_PHCTNCOA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_PHCTNCOB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F07btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('M_PHCTNCOA').getValue();
                    v_str.replace("'", "’");
                    T1Form.getForm().findField('M_PHCTNCOB').setValue(v_str);
                }
            }, {
                xtype: 'label',
                text: '廠牌',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_AGENLABA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_AGENLABB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F08btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('M_AGENLABB').setValue(T1Form.getForm().findField('M_AGENLABA').getValue());
                }
            }, {
                xtype: 'label',
                text: '申購包裝轉換率',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'UNIT_SWAP',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'EXCH_RATIO'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F09btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('M_AGENNOA').getValue();
                    v_str.replace(" ", "");
                    T1Form.getForm().findField('M_AGENNOB').setValue(T1Form.getForm().findField('M_AGENNOA').getValue());
                    T1Form.getForm().findField('M_PURUNB').setValue(T1Form.getForm().findField('M_PURUNA').getValue());
                    T1Form.getForm().findField('EXCH_RATIO').setValue(T1Form.getForm().findField('UNIT_SWAP').getValue());
                }
            }, {
                xtype: 'label',
                text: '折讓比(優惠%)',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_DISCPERCA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_DISCPERCB',
                maxLength: 10,
                maskRe: /[0-9]/
                ,
                listeners: {
                    change: function () {
                        getDISC_UPRICE();
                    }
                }
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F10btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('M_DISCPERCB').setValue(T1Form.getForm().findField('M_DISCPERCA').getValue());
                    getDISC_UPRICE();
                }
            }, {
                xtype: 'label',
                text: '優惠合約價',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'DISC_CPRICEA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'DISC_CPRICEB',
                //maxLength: 10,
                maskRe: /[0-9]/
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F11btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('DISC_CPRICEB').setValue(T1Form.getForm().findField('DISC_CPRICEA').getValue());
                }
            }, {
                xtype: 'label',
                text: '優惠計量(最小)單價',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'DISC_UPRICE'
            }, {
                xtype: 'label',
                text: '',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '計量(最小)單價',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'UPRICEA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'UPRICEB',
                //maxLength: 10,
                maskRe: /[0-9]/
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F12btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('UPRICEB').setValue(T1Form.getForm().findField('UPRICEA').getValue());
                }
            }, {
                xtype: 'label',
                text: '合約單價',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_CONTPRICEA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_CONTPRICEB',
                //maxLength: 10,
                maskRe: /[0-9]/
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F13btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('M_CONTPRICEB').setValue(T1Form.getForm().findField('M_CONTPRICEA').getValue());
                }
            }, {
                xtype: 'label',
                text: '健保碼',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_NHIKEYA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_NHIKEYB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F14btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    T1Form.getForm().findField('M_NHIKEYB').setValue(T1Form.getForm().findField('M_NHIKEYA').getValue());
                }
            }, {
                xtype: 'label',
                text: '廠商代碼',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_AGENNOA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_AGENNOB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F15btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    v_str = T1Form.getForm().findField('M_AGENNOA').getValue();
                    v_str.replace(" ", "");
                    T1Form.getForm().findField('M_AGENNOB').setValue(v_str);
                    T1Form.getForm().findField('M_PURUNB').setValue(T1Form.getForm().findField('M_PURUNA').getValue());
                    T1Form.getForm().findField('EXCH_RATIO').setValue(T1Form.getForm().findField('UNIT_SWAP').getValue());
                }
            }, {
                xtype: 'label',
                text: '標案來源',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_MATIDA',
                readOnly: true
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_MATIDB'
            }, {
                xtype: 'button',
                text: '複製',
                padding: '0 20 5 20',
                id: 'F16btn',
                labelAlign: 'center',
                handler: function () {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    var v_str = "";
                    var v_str2 = "";
                    var v_str3 = "";
                    v_str = T1Form.getForm().findField('M_MATIDA').getValue();
                    if (v_str == "1" || v_str == "2") {
                        v_str2 = "0";
                        v_str3 = "Y";
                    }
                    else {
                        if (v_str == "0") {
                            v_str2 = "3";
                            v_str3 = "N";
                        }
                        else {
                            v_str2 = "2";
                            v_str3 = "N";
                        }
                    }
                    T1Form.getForm().findField('M_MATIDB').setValue(v_str2);
                    T1Form.getForm().findField('M_CONTIDB').setValue(v_str3);
                }
            }, {
                xtype: 'label',
                text: '合約識別碼',
                padding: '0 20 5 20'
            }, {
                xtype: 'label',
                text: '',
                padding: '0 20 5 20'
            }, {
                fieldLabel: '',
                padding: '0 20 5 20',
                name: 'M_CONTIDB'
            }

        ]
    });
    function getDISC_UPRICE() {
        var objP0 = T1Form.getForm().findField("UPRICEB");//計量(最小)單價
        var objP1 = T1Form.getForm().findField("M_DISCPERCB");//折讓比(優惠%)
        var objP2 = T1Form.getForm().findField("M_CONTPRICEB");//合約單價
        var objP5 = T1Form.getForm().findField("DISC_UPRICE");//優惠計量(最小)單價=計量(最小)單價*(1-優惠%)
        var objP6 = T1Form.getForm().findField("DISC_CPRICEB");//優惠合約價=合約單價*(1-優惠%)
        if (objP0.getValue() != '' && objP1.getValue() != '') {
            objP5.setValue(Math.round((parseFloat(objP0.getValue()) * ( 100 - parseFloat(objP1.getValue()) ) / 100) * 10000) / 10000);
        }
        if (objP2.getValue() != '' && objP1.getValue() != '') {
            objP6.setValue(Math.round((parseFloat(objP2.getValue()) * (100 - parseFloat(objP1.getValue())) / 100) * 10000) / 10000);
        }
    }
    function T1FormBtnEnable(id) {
        if (id == '1') { //Enable
            Ext.getCmp('btnUpdate').setDisabled(false);
            Ext.getCmp('F01btn').setDisabled(false);
            Ext.getCmp('F02btn').setDisabled(false);
            Ext.getCmp('F03btn').setDisabled(false);
            Ext.getCmp('F04btn').setDisabled(false);
            Ext.getCmp('F05btn').setDisabled(false);
            Ext.getCmp('F06btn').setDisabled(false);
            Ext.getCmp('F07btn').setDisabled(false);
            Ext.getCmp('F08btn').setDisabled(false);
            Ext.getCmp('F09btn').setDisabled(false);
            Ext.getCmp('F10btn').setDisabled(false);
            Ext.getCmp('F11btn').setDisabled(false);
            Ext.getCmp('F12btn').setDisabled(false);
            Ext.getCmp('F13btn').setDisabled(false);
            Ext.getCmp('F14btn').setDisabled(false);
            Ext.getCmp('F15btn').setDisabled(false);
            Ext.getCmp('F16btn').setDisabled(false);
        }
        else {//Disabled
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('F01btn').setDisabled(true);
            Ext.getCmp('F02btn').setDisabled(true);
            Ext.getCmp('F03btn').setDisabled(true);
            Ext.getCmp('F04btn').setDisabled(true);
            Ext.getCmp('F05btn').setDisabled(true);
            Ext.getCmp('F06btn').setDisabled(true);
            Ext.getCmp('F07btn').setDisabled(true);
            Ext.getCmp('F08btn').setDisabled(true);
            Ext.getCmp('F09btn').setDisabled(true);
            Ext.getCmp('F10btn').setDisabled(true);
            Ext.getCmp('F11btn').setDisabled(true);
            Ext.getCmp('F12btn').setDisabled(true);
            Ext.getCmp('F13btn').setDisabled(true);
            Ext.getCmp('F14btn').setDisabled(true);
            Ext.getCmp('F15btn').setDisabled(true);
            Ext.getCmp('F16btn').setDisabled(true);
        }
    }
    function T1Cleanup() {
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype == "datefield") {
        //        fc.readOnly = true;
        //    }
        //});
        T1Query.getForm().findField('P9').setValue('02 衛材');
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
            region: 'center',
            layout: 'fit',
            collapsible: false,
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Form]
        }]
    });

    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P9').setValue('02 衛材');
    T1FormBtnEnable('2');
});
