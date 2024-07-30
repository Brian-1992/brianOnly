Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Get = '/api/AB0040/AllM'; // 查詢(改為於store定義)
    var T1Set = '';
    var T1Name = "藥學基本檔維護作業(DI藥師)";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var ActNumber = 0; // 目前在處理T1Store的第幾筆
    var LastNumber = 0;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_ordercode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0040/GetOrdercodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        fieldLabel: '院內碼',
        name: 'P0',
        id: 'P0',
        allowBlank: false,
        fieldCls: 'required',
        labelAlign: 'right',
        labelWidth: 60,
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0040/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {

            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                //T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
            }
        },
        padding: '0 4 0 4'
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    //{
                    //    xtype: 'combo',
                    //    fieldLabel: '院內碼',
                    //    name: 'P0',
                    //    id: 'P0',
                    //    store: st_ordercode,
                    //    queryMode: 'local',
                    //    displayField: 'TEXT',
                    //    valueField: 'VALUE',
                    //    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    //    labelAlign: 'right',
                    //    labelWidth: 60,
                    //    width: 180,
                    //    fieldCls: 'required',
                    //    padding: '0 4 0 4'
                    //},
                    T1QueryMMCode,
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            if (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null) 
                                Ext.Msg.alert('提醒', '院內碼不可空白');
                            else {
                                
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
                            T1Form.getForm().reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });
    var T1Store = Ext.create('WEBAPP.store.AB0040', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
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
                        Ext.Msg.alert('提醒', '查無資料!');
                        T1Form.getForm().reset();
                    }
                }
                T1Form.loadRecord(T1Store.data.items[0]);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    var containerwidth = 1000;
    var mLabelWidth = 90;
    var mWidth = 180;
    var mWidth2 = mWidth * 1.6;
    var mWidth3 = mWidth * 2.4;
    var mWidth4 = mWidth * 3.2;
    var mWidth31 = mWidth / 3 * 1.5;
    var mWidth32 = 180;
    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        width: 400,
        layout: {
            type: 'table',
            columns: 4,
            border: true,
            bodyBorder: true,
            //tableAttrs: {
            //    style: {
            //        width: '100%'
            //    }
            //},
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }],
        items: [
            
            { fieldLabel: '院內代碼', name: 'ORDERCODE', readOnly: true },
            { fieldLabel: '健保碼', name: 'INSUORDERCODE', readOnly: true },
            { fieldLabel: '別名(院內名稱)', name: 'OrderHospName', readOnly: true },
            { fieldLabel: '中文名稱', name: 'MMNAME_C', readOnly: true },

            { fieldLabel: '英文名稱', name: 'MMNAME_E', readOnly: true, colspan: 2, width: mWidth3 },
            { fieldLabel: '簡稱', name: 'OrderEasyName', readOnly: true, colspan: 2, width: mWidth2 },

            { fieldLabel: '成份名稱', name: 'ScientificName', readOnly: true, colspan: 4, width: mWidth4 },

            { fieldLabel: '預設公藥分類', name: 'PUBLICDRUGCODE', readOnly: true },
            { fieldLabel: '管制用藥', name: 'RESTRICTCODE', readOnly: true, colspan: 2 },
            //限制設定
            {
                xtype: 'fieldset', title: '限制設定',colspan: 1, rowspan: 5,width: 320, layout: {type: 'table',columns: 2},
                items: [
                    { xtype: 'textfield', fieldLabel: '限制重覆醫令', labelAlign: 'right', name: 'RESTRICTTYPE', readOnly: true, colspan: 2, width: 220, labelWidth: 80 },
                    { xtype: 'textfield', fieldLabel: '限制次數', labelAlign: 'right', name: 'MAXTAKETIMES', readOnly: true, colspan: 2, width: 220, labelWidth: 80 },

                    { xtype: 'textfield', fieldLabel: '門診開立數量', labelAlign: 'right', name: 'MAXQTYO', readOnly: true, colspan: 1, width: 140, labelWidth: 80 },
                    { xtype: 'textfield', fieldLabel: '住院開立數量', labelAlign: 'right', name: 'MAXQTYI', readOnly: true, colspan: 1, width: 140, labelWidth: 80 },

                    { xtype: 'textfield', fieldLabel: '門診開立日數', labelAlign: 'right', name: 'MAXDAYSO', readOnly: true, colspan: 1, width: 140, labelWidth: 80 },
                    { xtype: 'textfield', fieldLabel: '住院開立日數', labelAlign: 'right', name: 'MAXDAYSI', readOnly: true, colspan: 1, width: 140, labelWidth: 80 },

                    { xtype: 'textfield', fieldLabel: '門診效期日數', labelAlign: 'right', name: 'VALIDDAYSO', readOnly: true, colspan: 1, width: 140, labelWidth: 80 },
                    { xtype: 'textfield', fieldLabel: '住院效期日數', labelAlign: 'right', name: 'VALIDDAYSI', readOnly: true, colspan: 1, width: 140, labelWidth: 80 }
                ]
            },
            

            { fieldLabel: '預設給藥途徑', name: 'PATHNO', readOnly: true },
            {
                xtype: 'container', colspan: 2, width: mWidth2,
                items: [
                    {
                        xtype: 'checkboxfield', boxLabel: '限制途徑', id: 'FIXPATHNOFLAG', name: 'FIXPATHNOFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ',inputValue: 'Y' }
                ]
            },
            { fieldLabel: '預設劑量', name: 'DOSE', readOnly: true },
            {
                xtype: 'container', colspan: 3, width: mWidth3, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '限制劑量', id: 'HOSPEXAMINEQTYFLAG', name: 'HOSPEXAMINEQTYFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: '盤點品項', id: 'INVENTORYFLAG', name: 'INVENTORYFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },

            { fieldLabel: '住院預設頻率', name: 'FREQNOI', readOnly: true },
            { fieldLabel: '門診預設頻率', name: 'FREQNOO', readOnly: true },
            {
                xtype: 'container', colspan: 2, width: mWidth2,
                items: [
                    { xtype: 'checkboxfield', boxLabel: '研究用藥', id: 'RESEARCHDRUGFLAG', name: 'RESEARCHDRUGFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },

            { fieldLabel: '服用藥別(排序)', name: 'TAKEKIND', readOnly: true },
            { fieldLabel: '藥品類別', name: 'DRUGCLASS', readOnly: true },
            {
                xtype: 'container', colspan: 3, width: mWidth2, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '罕見疾病用藥', id: 'RAREDISORDERFLAG', name: 'RAREDISORDERFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },

            { fieldLabel: '藥品劑型', name: 'DRUGFORM', readOnly: true },
            { fieldLabel: '抗生素等級', name: 'ANTIBIOTICSCODE', readOnly: true },
            {
                xtype: 'container', colspan: 2, width: mWidth2, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '高價用藥', id: 'HIGHPRICEFLAG', name: 'HIGHPRICEFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },

            { fieldLabel: '外審(健保專案)碼', name: 'SPECIALORDERKIND', readOnly: true },
            {
                xtype: 'container', colspan: 3, width: mWidth3, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '開立限制設定', id: 'LIMITFLAG', name: 'LIMITFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: 'CDC藥品', id: 'RETURNDRUGFLAG', name: 'RETURNDRUGFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: '獨立處方箋', id: 'SINGLEITEMFLAG', name: 'SINGLEITEMFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: '保險給付', id: 'INSUOFFERFLAG', name: 'INSUOFFERFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: '公藥(hp)', id: 'PUBLICDRUGFLAG', name: 'PUBLICDRUGFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },

            {
                xtype: 'container', colspan: 1, width: mWidth, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '內審用藥', id: 'HOSPEXAMINEFLAG', name: 'HOSPEXAMINEFLAG', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' },
                    { xtype: 'checkboxfield', boxLabel: '限制用量', id: 'HospExamineQtyFlag', name: 'HospExamineQtyFlag', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },
            { fieldLabel: 'BC肝用藥註記', name: 'HEPATITISCODE', readOnly: true },
            {
                xtype: 'container', colspan: 1, width: mWidth, 
                items: [
                    { xtype: 'checkboxfield', boxLabel: '疫苗註記', id: 'VACCINE', name: 'VACCINE', readOnly: true, labelSeparator: '', fieldLabel: '    ', inputValue: 'Y' }
                ]
            },
            { fieldLabel: '疫苗類別', name: 'VACCINECLASS', readOnly: true },

            { fieldLabel: '藥品劑型規格(電子簽章)', labelWidth: 180, name: 'ORDERABLEDRUGFORM', readOnly: true, colspan: 4, width: mWidth4 }
        ]
    });

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
});
